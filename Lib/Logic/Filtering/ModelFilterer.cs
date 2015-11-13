using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Building.SemanticTree;

namespace Logic.Filtering
{
    public static class ModelFilterer
    {
        public static void ApplyFilter(ref Tree tree, Filters filters)
        {
            tree.SetChildren(tree.Childs.Select(FindSiblingDependencies));
            tree.SetChildren(SiblingReordrer.OrderChildsBySiblingsDependencies(tree.Childs));

            if (filters.RemoveTests)
                tree = RemoveTests(tree);
            if (filters.RemoveDefaultNamespaces)
                RemoveDefaultNamespaces(tree);
            if (filters.MaxDepth > 0)
                RemoveNodesWithMoreDepthThan(tree, filters.MaxDepth);
            if (filters.RemoveExceptions)
                RemoveExceptions(tree);
            if (filters.MinReferences > 0)
                RemoveNodesReferencedLessThan(tree, filters.MinReferences);
            if (filters.RemoveSinglePaths)
                tree = RemoveSinglePaths(tree);
            tree = RemoveSingleChildAnonymous(tree);
            //tree = FindSiblingPatterns(tree);
            //tree = RemoveSinglePaths(tree); change to remove projects/namespaces containing zero classes
        }

        private static void RemoveExceptions(Tree tree)
        {
            foreach (var child in tree.Childs.ToList())
            {
                if (child is ClassNode)
                {
                    var cls = child as ClassNode;
                    if(cls.BaseClasses.Any(x => x.ToString() == "Exception"))
                        tree.RemoveChild(cls);
                }
                RemoveExceptions(child);
            }
        }

        private static void RemoveDefaultNamespaces(Tree tree)
        {
            var projects = tree.Projects();
            var withDefaultNamespaces = projects.Where(p => p.Childs.Count() == 1 && p.Childs.First().Name == p.Name);
            foreach (var projectNode in withDefaultNamespaces)
            {
                var childNode = projectNode.Childs.First();
                projectNode.Orientation = childNode.Orientation;
                projectNode.SetChildren(childNode.Childs);
            }
        }

        public static void RemoveNodesWithMoreDepthThan(Tree tree, int maxDepth, int currDepth = -1)
        {
            if (!(tree is SiblingHolderNode))
                currDepth += 1;

            if (currDepth == maxDepth)
                tree.SetChildren(new List<Node>());
            else
            {
                foreach (var child in tree.Childs)
                {
                    RemoveNodesWithMoreDepthThan(child, maxDepth, currDepth);
                }
            }
        }

        private static void RemoveNodesReferencedLessThan(Tree tree, int byReference)
        {
            /*tree.SetChildren(tree.Childs.Where(x => !(x is ClassNode) || (x as ClassNode).References.Count() >= byReference));
            foreach (var child in tree.Childs)
            {
                RemoveNodesReferencedLessThan(child, byReference);
            }*/
        }


        public static Tree RemoveSinglePaths(Tree tree)
        {
            tree.SetChildren(tree.Childs.Select(RemoveSinglePaths).Cast<Node>().ToList());
            if (tree.Childs.Count == 1)
            {
                var node = tree as Node;
                var newTree = tree.Childs.First();
                if (node != null)
                {
                    newTree.SiblingDependencies.UnionWith(node.SiblingDependencies);
                    var dependsOnNode = node.Parent.Childs.Where(x => x.SiblingDependencies.Contains(tree));
                    foreach (var dependant in dependsOnNode)
                    {
                        dependant.SiblingDependencies.Remove(node);
                        dependant.SiblingDependencies.Add(newTree);
                    }
                }
                return newTree;
            }
            return tree;
        }

        public static Tree RemoveSingleChildAnonymous(Tree tree)
        {
            tree.SetChildren(tree.Childs.Select(RemoveSingleChildAnonymous).Cast<Node>().ToList());
            if (tree is SiblingHolderNode && tree.Childs.Count == 1)
            {
                var newTree = tree.Childs.First();
                return newTree;
            }
            return tree;
        }

        private static Tree RemoveTests(Tree tree)
        {
            tree.SetChildren(tree.Childs.Select(RemoveTests).Cast<Node>());
            tree.SetChildren(
                tree.Childs.Where(x => !x.Name.EndsWith("test", StringComparison.InvariantCultureIgnoreCase)).ToList());
            tree.SetChildren(
                tree.Childs.Where(x => !x.Name.EndsWith("tests", StringComparison.InvariantCultureIgnoreCase)).ToList());
            return tree;
        }

        private static Tree FindSiblingPatterns(Tree tree)
        {
            if (!tree.Childs.Any())
                return tree;
            if(!tree.Childs.Select(x => x.HasChildren()).Any()) { 
                var baseClassPattern = PatternFinder.FindBaseClassPattern(tree.Childs);
                if (baseClassPattern != null)
                {
                    tree.SetChildren(new List<Node>());
                    tree.AddChild(new Node(baseClassPattern + "s"));
                }
                else
                {
                    var namingPattern = PatternFinder.FindNamingPattern(tree.Childs.Select(x => x.Name));
                    if (namingPattern != null)
                    {
                        tree.SetChildren(new List<Node>());
                        tree.AddChild(new Node(namingPattern + "s"));
                    }
                }
            }
            tree.SetChildren(tree.Childs.Select(FindSiblingPatterns).Cast<Node>());
            return tree;
        }

        public static Node FindSiblingDependencies(Node node)
        {
            node.SetChildren(node.Childs.Select(FindSiblingDependencies).ToList());
            if (node.Parent == null) return node;
            var allSubDependencies = node.AllSubDependencies().ToList();
            var dependenciesWhereAncestorsAreSiblings = allSubDependencies
                .Select(dependency => AncestorIsSibling(node.Parent, dependency)).ToList();
            foreach (
                var sibling in
                    dependenciesWhereAncestorsAreSiblings
                        .Where(sibling => sibling != null && sibling != node))
            {
                node.SiblingDependencies.Add(sibling);
            }

            return node;
        }

        public static Node AncestorIsSibling(Tree parent, Node dependency)
        {
            for (var p = dependency; p != null; p = p.Parent as Node)
            {
                if (p.Parent.Id == parent.Id)
                    return p;
            }
            return null;
        }
    }
}