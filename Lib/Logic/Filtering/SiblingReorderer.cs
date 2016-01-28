using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Logic.Building;
using Logic.Common;
using Logic.SemanticTree;
using static Logic.SemanticTree.OrientationKind;

namespace Logic.Filtering
{
    public static class SiblingReorderer
    {
        public static IEnumerable<Node> OrderChildsBySiblingsDependencies(IReadOnlyList<Node> childs)
        {
            foreach (var child in childs.Where(child => child.Childs.Any()))
            {
                child.SetChildren(OrderChildsBySiblingsDependencies(child.Childs));
            }

            if (!childs.SiblingDependencies().Any())
            {
                if (childs.Any() && childs.First().Parent != null)
                    childs.First().Parent.Orientation = Horizontal;
                return childs;
            }

            var oldChildList = childs.ToList();
            var newChildOrder = GroupNodes(oldChildList);
            return newChildOrder;
        }

        private static IEnumerable<Node> GroupNodes(List<Node> oldChildList)
        {
            FindCircularReferences(ref oldChildList);
            return RegroupSiblingNodes(oldChildList);
        }

        public static List<Node> GetFacadeNodes(List<Node> targets)
        {
            var allDependencies = targets.SiblingDependencies().ToList();
            return targets.Where(n => !allDependencies.Contains(n)).ToList();
        }

        public static List<Node> RegroupSiblingNodes(List<Node> oldChildList)
        {
            if (oldChildList.Count <= 1)
                return oldChildList;

            var newChildOrder = new List<Node>();
            var target = oldChildList;
            var unreferenced = oldChildList.Where(x => !x.SiblingDependencies.Any()
                                                       && !oldChildList.SiblingDependencies().Contains(x)).ToList();
            target.RemoveRange(unreferenced);
            while (oldChildList.Any())
            {
                if (!target.Any())
                {
                    target = oldChildList;
                }
                while (target.Any())
                {
                    var firstLayer = GetFacadeNodes(target);
                    var nextLayer = firstLayer.SiblingDependencies().ToList();
                    nextLayer = GetFacadeNodes(nextLayer);
                    //Get nodes that are not dependencies to all in the previous layer
                    var uniqueDependencies =
                        nextLayer.Where(x => !firstLayer.All(n => n.SiblingDependencies.Contains(x)))
                            .ToImmutableHashSet();


                    target = firstLayer.SiblingDependencies().ToList();

                    if (uniqueDependencies.Any())
                    {
                        var dependencyGroups = FindDependencyGroups(firstLayer, nextLayer);
                        oldChildList.RemoveRange(firstLayer);
                        foreach (var dependencyGroup in dependencyGroups)
                        {
                            firstLayer.RemoveRange(dependencyGroup.Referencers.ToList());
                            var dependants = dependencyGroup.Dependants.ToList();
                            target.RemoveRange(dependants);
                            oldChildList.RemoveRange(dependants);
                            var depNode = CreateHorizontalLayer(dependants);
                            var referenceNode = CreateHorizontalLayer(dependencyGroup.Referencers);
                            var newList = new List<Node> {depNode, referenceNode};
                            //Needs to be only those that are unique dependencies
                            var depNodeDependencies = dependants.SiblingDependencies().ToList();
                            //Get the other groups
                            var otherGroups = dependencyGroups.Except(dependencyGroup);
                            var nodesInOtherGroups = otherGroups.SelectMany(x => x.Dependants.Union(x.Referencers));
                            var otherNodes =
                                oldChildList.Union(nodesInOtherGroups).Union(firstLayer).Except(depNodeDependencies);
                            var nonUniqueDependencies = otherNodes.SiblingDependencies();
                            //Remove nodes that are dependency for a node in other vertical layer or left in childlist (except for those in depNodeDep)
                            depNodeDependencies.RemoveRange(
                                depNodeDependencies.Where(x => nonUniqueDependencies.Contains(x)).ToList());

                            foreach (var depNodeDependency in depNodeDependencies)
                            {
                                depNodeDependency.SiblingDependencies.RemoveWhere(x => !depNodeDependencies.Contains(x));
                            }
                            //Should be any of the verticals
                            depNodeDependencies.RemoveRange(
                                depNodeDependencies.Where(x => oldChildList.Any(y => y.DependsOn(x))).ToList());
                            if (depNodeDependencies.Any())
                            {
                                oldChildList.RemoveRange(depNodeDependencies);
                                target.RemoveRange(depNodeDependencies);
                                if (!depNodeDependencies.SiblingDependencies().Any())
                                    depNodeDependencies = new List<Node> {CreateHorizontalLayer(depNodeDependencies)};
                                else
                                    depNodeDependencies = GroupNodes(depNodeDependencies).Reverse().ToList();
                                newList.InsertRange(0, depNodeDependencies);
                            }
                            firstLayer.Add(new VerticalSiblingHolderNode(newList));
                        }
                    }
                    else
                    {
                        oldChildList.RemoveRange(firstLayer);
                    }
                    newChildOrder.Add(CreateHorizontalLayer(firstLayer));
                }
            }
            //TODO: handle this or just skip?
            //if (unreferenced.Any())
            //    newChildOrder.Add(CreateHorizontalLayer(unreferenced));

            newChildOrder.Reverse();
            return newChildOrder;
        }

        public static IEnumerable<Dependency> FindDependencies(List<Node> firstLayer, List<Node> nextLayer)
        {
            //Get nodes that are not dependencies to all in the previous layer
            var uniqueDependencies =
                nextLayer.Where(x => !firstLayer.All(n => n.SiblingDependencies.Contains(x))).ToImmutableHashSet();
            var dependencies = new List<Dependency>();
            foreach (var dependency in uniqueDependencies)
            {
                var referencers = firstLayer.Where(x => x.DependsOn(dependency));
                dependencies.AddRange(referencers.Select(x => new Dependency(dependency, x)));
            }
            return dependencies;
        }

        public static IEnumerable<DependencyGroup> FindPotentialDependencyGroups(List<Dependency> dependencies)
        {
            var groups = dependencies.Select(x => x.ToDependencyGroup()).ToList();
            var referencers = dependencies.Select(d => d.Referencer).Distinct();

            //Merge dependency groups
            foreach (var referencer in referencers)
            {
                //Get all groups for this referencer
                var relevantGroups = dependencies.Where(dep => dep.Referencer == referencer).ToList();
                if (relevantGroups.Count() > 1)
                {
                    //Combine all its dependencies
                    groups.Add(new DependencyGroup(
                        relevantGroups.Select(g => g.Dependant),
                        new List<Node> {referencer})
                        );
                }
            }

            var uniqueDependantGroups = new List<ISet<Node>>();
            var allDependants = groups.Select(x => x.Dependants).ToList();
            foreach (var dependants in allDependants)
            {
                //Discard duplicates
                if (uniqueDependantGroups.Any(x => x.SetEquals(dependants)))
                    continue;
                uniqueDependantGroups.Add(dependants);
                var relevantGroups = groups.Where(group => group.Dependants.SetEquals(dependants)).ToList();
                if (relevantGroups.Count() > 1)
                {
                    groups.Add(new DependencyGroup(dependants,
                        relevantGroups.SelectMany(x => x.Referencers)));
                }
            }

            return groups;
        }

        public static IReadOnlyCollection<DependencyGroup> FindDependencyGroups(List<Node> firstLayer,
            List<Node> nextLayer)
        {
            var dependencies = FindDependencies(firstLayer, nextLayer).ToList();
            var potentialGroups = FindPotentialDependencyGroups(dependencies).ToList();
            foreach (var group in potentialGroups.ToList())
            {
                var referencers = group.Referencers;
                var dependants = group.Dependants;
                //If dependency exists for any other referencer not contained in referencers, its invalid
                if (potentialGroups.Any(group2 => dependants.Intersect(group2.Dependants).Any()
                                                  &&
                                                  group2.Referencers.Any(x => !referencers.Contains(x))))
                {
                    potentialGroups.Remove(group);
                }
            }

            var patternsToUse = new List<DependencyGroup>();

            CalculatePatternScore(ref potentialGroups);

            while (potentialGroups.Any())
            {
                var biggest = potentialGroups.OrderByDescending(x => x.Score).First();
                var madeInvalid =
                    potentialGroups.Where(g => g != biggest && g.Referencers.ContainsAnyFrom(biggest.Referencers));
                patternsToUse.Add(biggest);
                potentialGroups.RemoveRange(madeInvalid.ToList());
                potentialGroups.Remove(biggest);
            }

            return patternsToUse;
        }

        public static void CalculatePatternScore(ref List<DependencyGroup> dependencyGroups)
        {
            foreach (var group in dependencyGroups)
            {
                var collidingPatterns = dependencyGroups.Count(group2 => group2 != group
                                                                         &&
                                                                         group.Referencers.ContainsAnyFrom(
                                                                             group2.Referencers));
                group.Score = -collidingPatterns + group.Dependants.Count + group.Referencers.Count;
            }
        }


        private static Node CreateHorizontalLayer(ICollection<Node> uniqueReferencers)
        {
            return uniqueReferencers.Count == 1
                ? uniqueReferencers.First()
                : new HorizontalSiblingHolderNode(uniqueReferencers);
        }

        public static void RegroupSiblingNodes2(List<Node> oldChildList,
            ref List<Node> newChildOrder)
        {
            var nodesWithoutDependency = new List<Node>();
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
                if (!nodesWithoutDependency.Any())
                {
                    if (oldChildList.Count == 2)
                        nodesWithoutDependency = oldChildList.ToList();
                    else
                        throw new LayerViolationException();
                }

                var newNode = nodesWithoutDependency.Count == 1
                    ? nodesWithoutDependency.First()
                    : new HorizontalSiblingHolderNode(nodesWithoutDependency);
                newChildOrder.Add(newNode);
                foreach (var node in nodesWithoutDependency)
                {
                    oldChildList.Remove(node);
                }
            }
        }

        public static void FindCircularReferences(ref List<Node> childList)
        {
            FindCircularReferences(childList, childList);
        }

        private static void FindCircularReferences(List<Node> childList1, List<Node> childList2)
        {
            var circularRefs = new List<Tuple<Node, Node>>();
            foreach (var node in childList1)
            {
                foreach (var node2 in from node2 in childList2.Where(n => n != node)
                    where node.SiblingDependencies.Contains(node2)
                    where node2.SiblingDependencies.Contains(node)
                    where circularRefs.All(x => x.Item1 != node2 && x.Item2 != node)
                    select node2)
                {
                    circularRefs.Add(new Tuple<Node, Node>(node, node2));
                }
            }
            var circularDependencyHolders = new List<Node>();
            //NEEED TO CHECK FOR CIRULARREFERENCES WITH THE NEWLY CREATED CIRULARDEPENDENCYHOLDERS
            foreach (var circularRef in circularRefs)
            {
                var circularDependencyHolderNode =
                    new CircularDependencyHolderNode(new List<Node> {circularRef.Item1, circularRef.Item2});
                circularDependencyHolderNode.SiblingDependencies.UnionWith(
                    circularRef.Item1.SiblingDependencies.Union(circularRef.Item2.SiblingDependencies));
                childList2.Add(circularDependencyHolderNode);
                circularDependencyHolderNode.SiblingDependencies.Remove(circularRef.Item1);
                circularDependencyHolderNode.SiblingDependencies.Remove(circularRef.Item2);
                childList2.Remove(circularRef.Item1);
                childList2.Remove(circularRef.Item2);
                foreach (var node3 in childList2.Where(n => n != circularDependencyHolderNode))
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
                circularDependencyHolders.Add(circularDependencyHolderNode);
            }
            if (circularDependencyHolders.Any() && circularDependencyHolders != childList2)
                FindCircularReferences(circularDependencyHolders, childList2);
        }

        public class DependencyGroup
        {
            public readonly ISet<Node> Dependants;
            public readonly ISet<Node> Referencers;
            public int Score;

            public DependencyGroup(IEnumerable<Node> dependants, IEnumerable<Node> referencers)
            {
                Referencers = referencers.ToImmutableHashSet();
                Dependants = dependants.ToImmutableHashSet();
            }
        }

        public class Dependency
        {
            public readonly Node Dependant;
            public readonly Node Referencer;

            public Dependency(Node dependant, Node referencer)
            {
                Dependant = dependant;
                Referencer = referencer;
            }

            public DependencyGroup ToDependencyGroup()
            {
                return new DependencyGroup
                    (new List<Node> {Dependant},
                        new List<Node> {Referencer});
            }
        }
    }
    
    internal class LayerViolationException : Exception
    {
    }
}