using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Ordering;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering
{
    public abstract class ChildrenFilter
    {
        protected Func<ClassNode, bool> Predicate;

        public static implicit operator Func<ClassNode, bool>(ChildrenFilter c)
        {
            return c.Predicate;
        }
    }

    public class SmallClassFilter : ChildrenFilter
    {
        public SmallClassFilter(int nrOfMethodsMin)
        {
            Predicate = node => node.NrOfMethods < nrOfMethodsMin;
        }
    }

    public static class ClassFilters
    {
        public static Func<ClassNode, bool> Exceptions = x => x.BaseClasses.Any(y => y.ToString() == "Exception");
    }

    public static class NodeFilters
    {
        public static Func<Node, bool> Tests = x =>
            x.Name.EndsWith("test", StringComparison.InvariantCultureIgnoreCase)
            || x.Name.EndsWith("tests", StringComparison.InvariantCultureIgnoreCase);
    }

    public class ModelFilterer
    {
        public static void ApplyNodeFilter(Node t, Func<Node, bool> filter)
        {
            var toRemove = t.Childs.Where(filter).ToList();
            toRemove.ForEach(t.RemoveChild);
            t.Childs.ForEach(x => ApplyNodeFilter(x, filter));
        }
        public static void ApplyClassFilter(Node t, Func<ClassNode, bool> filter)
        {
            var classes = t.Childs.OfType<ClassNode>();
            var toRemove = classes.Where(filter).ToList();
            toRemove.ForEach(t.RemoveChild);
            t.Childs.ForEach(x => ApplyClassFilter(x, filter));
        }
        //Everything that removes children of parent nodes needs to update their parent dependencies
        //Filters that pushes children upwards does not need to care
        public static void ApplyFilter(ref Node tree, Filters filters)
        {
            if (filters.RemoveTests)
                ApplyNodeFilter(tree, NodeFilters.Tests);
            if (filters.RemoveDefaultNamespaces)
                RemoveDefaultNamespaces(tree);

            if (filters.RemoveContainers)
            {
                if (filters.MaxDepth > 0)
                    RemoveNodesWithMoreDepthThan(tree, filters.MaxDepth);
                if (filters.MinReferences > 0)
                    RemoveNodesReferencedLessThan(tree, filters.MinReferences);
                RemoveContainers(ref tree);
            }

            if (filters.RemoveExceptions)
                ApplyClassFilter(tree, ClassFilters.Exceptions);

            tree.SetChildren(tree.Childs.Select(FindSiblingDependencies));
            tree.SetChildren(SiblingReorderer.OrderChildsBySiblingsDependencies(tree.Childs));
            
            if (filters.MaxDepth > 0 && !filters.RemoveContainers)
                RemoveNodesWithMoreDepthThan(tree, filters.MaxDepth);
            if (filters.MinMethods > 0)
                ApplyClassFilter(tree, new SmallClassFilter(filters.MinMethods));
            if (filters.MinReferences > 0)
                RemoveNodesReferencedLessThan(tree, filters.MinReferences);
            if (filters.RemoveSinglePaths)
                RemoveSinglePaths(tree);
            RemoveSingleChildAnonymous(tree);
            if (filters.FindNamingPatterns)
                tree = FindSiblingPatterns(tree);
        }

        private static void RemoveContainers(ref Node tree)
        {
            tree.SetChildren(FindClasses(tree));
            //tree.SetChildren(tree.DescendantNodes().Where(c => !c.HasChildren()));
        }

        private static IEnumerable<ClassNode> FindClasses(Node tree)
        {
            foreach (var child in tree.Childs)
            {
                if (child is ClassNode)
                    yield return child as ClassNode;
                else
                {
                    foreach (var node in FindClasses(child))
                        yield return node;
                }
            }
        }

        private static void RemoveDefaultNamespaces(Node tree)
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

        public static void FindIndirectSiblingDeps(Node node)
        {
            foreach (var descendantNode in node.DescendantNodes())
            {
                descendantNode.IndirectSiblingDependencies = IndirectSiblingBuilder.BuildDepsFor(descendantNode);
            }
        }


        public static void RemoveNodesWithMoreDepthThan(Node tree, int maxDepth, int currDepth = -1)
        {
            if (!(tree is SiblingHolderNode))
                currDepth += 1;

            if (currDepth == maxDepth)
            {
                tree.Dependencies.AddRange(tree.Childs.SelectMany(x => x.AllSubDependencies()).Distinct());
                tree.SetChildren(new List<Node>());
            }
            else
            {
                foreach (var child in tree.Childs)
                {
                    RemoveNodesWithMoreDepthThan(child, maxDepth, currDepth);
                }
            }
        }

        private static void RemoveNodesReferencedLessThan(Node tree, int byReference)
        {
            //tree.SetChildren(tree.Childs.Where(x => tree.Childs.SiblingDependencies().Contains(x)));
            /*var allSubDependencies = tree.AllSubDependencies();
            tree.SetChildren(tree.Childs.Where(x => allSubDependencies.Contains(x)));
            foreach (var child in tree.Childs)
            {
                RemoveNodesReferencedLessThan(child, byReference);
            }*/

            /*tree.SetChildren(tree.Childs.Where(x => !(x is ClassNode) || (x as ClassNode).References.Count() >= byReference));
            foreach (var child in tree.Childs)
            {
                RemoveNodesReferencedLessThan(child, byReference);
            }*/
        }


        public static void RemoveSinglePaths(Node tree)
        {
            tree.Childs.ForEach(RemoveSinglePaths);

            foreach (var oldChild in tree.Childs.ToList())
            {
                if (tree.Childs.Count != 1) continue;
                var node = tree;
                var newChild = tree.Childs.First();
                {
                    newChild.SiblingDependencies.UnionWith(node.SiblingDependencies);
                    var dependsOnNode = node.Parent.Childs.Where(x => x.SiblingDependencies.Contains(tree));
                    foreach (var dependant in dependsOnNode)
                    {
                        dependant.SiblingDependencies.Remove(node);
                        dependant.SiblingDependencies.Add(newChild);
                    }
                }
                tree.ReplaceChild(oldChild,newChild);
            }
        }

        public static void RemoveSingleChildAnonymous(Node tree)
        {
            foreach (var oldChild in tree.Childs.ToList())
            {
                RemoveSingleChildAnonymous(oldChild);
                if (oldChild is SiblingHolderNode)
                {
                    if(!oldChild.Childs.Any())
                        tree.RemoveChild(oldChild);
                    if(oldChild.Childs.Count == 1)
                    { 
                        var newChild = oldChild.Childs?.First();
                        tree.ReplaceChild(oldChild, newChild);
                    }
                }
            }
        }

        private static Node FindSiblingPatterns(Node tree)
        {
            if (!tree.Childs.Any())
                return tree;
            if(!tree.Childs.Any(x => x.HasChildren())) { 
                var baseClassPattern = PatternFinder.FindBaseClassPattern(tree.Childs);
                if (baseClassPattern != null)
                {
                    tree.SetChildren(new List<Node>());
                    tree.AddChild(new Node(baseClassPattern + "s"));
                }
                else
                {
                    var namingPatterns = PatternFinder.FindNamingPatterns(tree.Childs.Select(x => x.Name)).ToList();
                    if (namingPatterns.Any())
                    {
                        foreach (var pattern in namingPatterns.ToList())
                        {
                            var followspattern = tree.Childs.Where(x => PatternFinder.FollowsPattern(x.Name,pattern)).ToList();
                            foreach (var node in followspattern)
                            {
                                tree.RemoveChild(node);
                            }
                            tree.AddChild(new Node(pattern + "s"));
                        }
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

        public static Node AncestorIsSibling(Node parent, Node dependency)
        {
            for (var p = dependency; p != null; p = p.Parent)
            {
                if (p.Parent?.Id == parent.Id)
                    return p;
            }
            return null;
        }
    }
}