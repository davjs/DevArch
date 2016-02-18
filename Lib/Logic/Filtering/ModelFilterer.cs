using System.Collections.Generic;
using System.Linq;
using Logic.Filtering.Filters;
using Logic.Ordering;
using Logic.SemanticTree;

namespace Logic.Filtering
{
    public static class ModelFilterer
    {
        //Everything that removes children of parent nodes needs to update their parent dependencies
        //Filters that pushes children upwards does not need to care
        public static Node ApplyFilters(this Node tree, IEnumerable<Filter> filters)
        {
            foreach (var filter in filters.Where(filter => filter.ShouldBeApplied))
            {
                filter.Apply(tree);
            }
            return tree;
        }

        public static Node RelayoutBasedOnDependencies(this Node n)
        {
            n.SetChildren(n.Childs.Select(FindSiblingDependencies));
            n.SetChildren(SiblingReorderer.OrderChildsBySiblingsDependencies(n.Childs));
            return n;
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

        /*
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
        }*/

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