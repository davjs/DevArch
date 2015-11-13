using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Logic.Building;
using Logic.Building.SemanticTree;
using static Logic.Building.SemanticTree.OrientationKind;

namespace Logic.Filtering
{
    public static class SiblingReordrer
    {
        public static IEnumerable<Node> OrderChildsBySiblingsDependencies(IReadOnlyList<Node> childs)
        {
            foreach (var child in childs)
            {
                if (child.Childs.Any())
                    child.SetChildren(OrderChildsBySiblingsDependencies(child.Childs));
            }

            if (!childs.SiblingDependencies().Any())
            {
                if (childs.Any() && childs.First().Parent != null)
                    childs.First().Parent.Orientation = Horizontal;
                return childs;
            }

            var oldChildList = childs.ToList();
            FindCircularReferences(ref oldChildList);
            var nodesWithoutDependency = oldChildList.Where(c => !c.SiblingDependencies.Any()).ToList();
            var newChildOrder = new List<Node>();
            if (nodesWithoutDependency.Count == 0)
                throw new LayerViolationException();
            RegroupSiblingNodes(oldChildList, ref newChildOrder);
            return newChildOrder;
        }

        public static List<Node> GetFacadeNodes(List<Node> targets)
        {
            var allDependencies = targets.SiblingDependencies().ToList();
            return targets.Where(n => !allDependencies.Contains(n)).ToList();
        }
        
        public static void RegroupSiblingNodes(List<Node> oldChildList,
            ref List<Node> newChildOrder)
        {
            var target = oldChildList;

            while (target.Any())
            {
                var firstLayer = GetFacadeNodes(target);
                var nextLayer = firstLayer.SiblingDependencies().ToList();
                nextLayer = GetFacadeNodes(nextLayer);
                //Get nodes that are not dependencies to all in the previous layer
                var uniqueDependencies = nextLayer.Where(x => !firstLayer.All(n => n.SiblingDependencies.Contains(x))).ToImmutableHashSet();
                
                //A X B
                if (uniqueDependencies.Any())
                {
                    //C D E
                    var uniqueReferencers = firstLayer.Where(x => uniqueDependencies.Any(d => x.SiblingDependencies.Contains(d))).ToList();

                    var refs = uniqueReferencers.ToDictionary(referencer => referencer.SiblingDependencies.Intersect(uniqueDependencies).ToImmutableHashSet());
                    var commonUniqueDependencies = new Dictionary<ISet<Node>, List<Node>>();

                    foreach (var key in refs.Keys)
                    {
                        var x = (from prevKey in commonUniqueDependencies.Keys where prevKey.SetEquals(key) select commonUniqueDependencies[prevKey]).FirstOrDefault();
                        if (x == null) { 
                            x = new List<Node>();
                            commonUniqueDependencies.Add(key,x);
                        }
                        x.Add(refs[key]);
                    }

                    foreach (var key in commonUniqueDependencies.Keys.ToList())
                    {
                        foreach (var key2 in commonUniqueDependencies.Keys.ToList())
                        {
                            if (key.IsProperSubsetOf(key2))
                            {
                                commonUniqueDependencies.Remove(key2);
                            }
                        }
                    }

                    foreach (var dependencyGroup in commonUniqueDependencies)
                    {
                        var referencers = dependencyGroup.Value;
                        foreach (var referencer in referencers)
                        {
                            firstLayer.Remove(referencer);
                        }
                        var referenceNode = CreateHorizontalLayer(referencers);
                        var depNode = CreateHorizontalLayer(dependencyGroup.Key);

                        var newList = new List<Node> {depNode, referenceNode};
                        firstLayer.Add(new VerticalSiblingHolderNode(newList));
                    }
                }

                newChildOrder.Add(CreateHorizontalLayer(firstLayer));
                target = firstLayer.SiblingDependencies().ToList();
            }

            newChildOrder.Reverse();
        }

        public class DependencyGroup
        {
            public ISet<Node> _referencers;
            public ISet<Node> _dependants;

            public DependencyGroup(ISet<Node> referencers, ISet<Node> dependants)
            {
                _referencers = referencers;
                _dependants = dependants;
            }
        }

        public class Dependency
        {
            public Node _dependant;
            public Node _referencer;

            public Dependency(Node dependant, Node referencer)
            {
                _dependant = dependant;
                _referencer = referencer;
            }
        }

        public static IEnumerable<Dependency> FindDependencies(List<Node> firstLayer, List<Node> nextLayer)
        {
            //Get nodes that are not dependencies to all in the previous layer
            var uniqueDependencies = nextLayer.Where(x => !firstLayer.All(n => n.SiblingDependencies.Contains(x))).ToImmutableHashSet();
            //var referencers = firstLayer.Where(x => uniqueDependencies.Any(x.DependsOn));
            var dependencies = new List<Dependency>();
            foreach (var dependency in uniqueDependencies)
            {
                var referencers = firstLayer.Where(x => x.DependsOn(dependency));
                dependencies.AddRange(referencers.Select(x => new Dependency(dependency,x)));
            }
            return dependencies;
        }

        public static IEnumerable<DependencyGroup> FindDependencyGroups(List<Node> firstLayer, List<Node> nextLayer)
        {
            var dependencies = FindDependencies(firstLayer, nextLayer);

            var groups = new List<DependencyGroup>();
            foreach (var dependency in dependencies)
            {
                groups.Where(g => g._dependants.Contains(dependency._dependant));
            }

        }

        private static Node CreateHorizontalLayer(ICollection<Node> uniqueReferencers)
        {
            return uniqueReferencers.Count == 1 ? uniqueReferencers.First() : new HorizontalSiblingHolderNode(uniqueReferencers);
        }

        /*public static void RegroupSiblingNodes(List<Node> nodesWithoutDependency, List<Node> oldChildList,
            ref List<Node> newChildOrder)
        {
            Node prevNode = null;
            while (oldChildList.Any())
            {
                foreach (var node1 in nodesWithoutDependency)
                {
                    foreach (var node in oldChildList)
                    {
                        node.SiblingDependencies.Remove(node1);
                    }
                }
                nodesWithoutDependency = oldChildList.Where(x => !x.SiblingDependencies.Any()).ToList();
                if (prevNode is SiblingHolderNode)
                {
                    //Check if the next layer has anything from the previous that its not dependant on
                    var UnusedDependency = prevNode.Childs.Where(c => !nodesWithoutDependency.Any(n => n.SiblingDependencies.Contains(c)));
                    var notDependantOnAllInPrevious = nodesWithoutDependency.Where(
                        n => !prevNode.Childs.ToImmutableHashSet().IsSubsetOf(n.SiblingDependencies));
                }
                if (!nodesWithoutDependency.Any())
                {
                    if (oldChildList.Count == 2)
                        nodesWithoutDependency = oldChildList.ToList();
                    else
                        throw new LayerViolationException();
                }

                var newNode = nodesWithoutDependency.Count == 1
                    ? nodesWithoutDependency.First()
                    : new SiblingHolderNode(nodesWithoutDependency);
                prevNode = newNode;
                newChildOrder.Add(newNode);
                foreach (var node in nodesWithoutDependency)
                {
                    oldChildList.Remove(node);
                }
            }
        }*/

        public static void FindCircularReferences(ref List<Node> childList)
        {
            var circularRefs = new List<Tuple<Node, Node>>();
            foreach (var node in childList)
            {
                foreach (var node2 in from node2 in childList.Where(n => n != node)
                    where node.SiblingDependencies.Contains(node2)
                    where node2.SiblingDependencies.Contains(node)
                    where circularRefs.All(x => x.Item1 != node2 && x.Item2 != node)
                    select node2)
                {
                    circularRefs.Add(new Tuple<Node, Node>(node, node2));
                }
            }

            foreach (var circularRef in circularRefs)
            {
                var circularDependencyHolderNode =
                    new CircularDependencyHolderNode(new List<Node> {circularRef.Item1, circularRef.Item2});
                circularDependencyHolderNode.SiblingDependencies.UnionWith(
                    circularRef.Item1.SiblingDependencies.Union(circularRef.Item2.SiblingDependencies));
                childList.Add(circularDependencyHolderNode);
                circularDependencyHolderNode.SiblingDependencies.Remove(circularRef.Item1);
                circularDependencyHolderNode.SiblingDependencies.Remove(circularRef.Item2);
                childList.Remove(circularRef.Item1);
                childList.Remove(circularRef.Item2);
                foreach (var node3 in childList.Where(n => n != circularDependencyHolderNode))
                {
                    var containedNode1 = node3.SiblingDependencies.Contains(circularRef.Item1);
                    var containedNode2 = node3.SiblingDependencies.Contains(circularRef.Item2);
                    if (containedNode1 || containedNode2)
                        node3.SiblingDependencies.Add(circularDependencyHolderNode);
                    if (containedNode1)
                        node3.SiblingDependencies.Remove(circularRef.Item1);
                    if (containedNode2)
                        node3.SiblingDependencies.Remove(circularRef.Item2);
                }
            }
        }
    }
}