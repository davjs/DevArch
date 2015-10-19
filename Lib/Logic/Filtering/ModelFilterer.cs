using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Analysis;
using Logic.Analysis.SemanticTree;

namespace Logic.Filtering
{
    public static class ModelFilterer
    {
        public static void ApplyFilter(ref Tree tree, Filters filters)
        {
            if(filters.RemoveTests)
                tree = RemoveTests(tree);
            tree.UpdateChildren(tree.Childs.Select(FindSiblingDependencies).ToList());
            if(filters.RemoveSinglePaths)
                tree = RemoveSinglePaths(tree);
            tree.UpdateChildren(SiblingReordrer.OrderChildsBySiblingsDependencies(tree.Childs).ToList());
            tree = FindSiblingPatterns(tree);
            if (filters.ByReference > 0)
                RemoveNodesReferencedLessThan(tree,filters.ByReference);
            //tree = RemoveSinglePaths(tree); change to remove projects/namespaces containing zero classes
        }

        private static void RemoveNodesReferencedLessThan(Tree tree, int byReference)
        {
            tree.UpdateChildren(tree.Childs.Where(x => !(x is ClassNode) || (x as ClassNode).References.Count() >= byReference));
            foreach (var child in tree.Childs)
            {
                RemoveNodesReferencedLessThan(child, byReference);
            }
        }


        public static Tree RemoveSinglePaths(Tree tree)
        {
            tree.UpdateChildren(tree.Childs.Select(RemoveSinglePaths).Cast<Node>().ToList());
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


        private static Tree RemoveTests(Tree tree)
            {
            tree.UpdateChildren(tree.Childs.Select(RemoveTests).Cast<Node>());
            tree.UpdateChildren(
                tree.Childs.Where(x => !x.Name.EndsWith("test", StringComparison.InvariantCultureIgnoreCase)).ToList());
            tree.UpdateChildren(
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
                    tree.UpdateChildren(new List<Node>());
                    tree.AddChild(new Node(baseClassPattern + "s"));
                }
                else
                {
                    var namingPattern = PatternFinder.FindNamingPattern(tree.Childs.Select(x => x.Name));
                    if (namingPattern != null)
                    {
                        tree.UpdateChildren(new List<Node>());
                        tree.AddChild(new Node(namingPattern + "s"));
                    }
                }
            }
            tree.UpdateChildren(tree.Childs.Select(FindSiblingPatterns).Cast<Node>());
            return tree;
        }

        public static Node FindSiblingDependencies(Node node)
        {
            node.UpdateChildren(node.Childs.Select(FindSiblingDependencies).ToList());
            if (node.Parent == null) return node;
            var allSubDependencies = node.AllSubDependencies().ToList();
            var dependenciesWhereAncestorsAreSiblings = allSubDependencies
                .Select<Node, Node>(dependency => AncestorIsSibling(node.Parent, dependency)).ToList();
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