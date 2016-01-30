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

        public static List<Node> RegroupSiblingNodes(List<Node> toBeGrouped)
        {
            if (toBeGrouped.Count <= 1)
                return toBeGrouped;

            var groupedNodes = new List<Node>();
            var nextGroupTarget = toBeGrouped;
            var unreferenced = toBeGrouped.Where(x => !x.SiblingDependencies.Any()
                                                       && !toBeGrouped.SiblingDependencies().Contains(x)).ToList();
            nextGroupTarget.RemoveRange(unreferenced);
            while (toBeGrouped.Any())
            {
                if (!nextGroupTarget.Any())
                {
                    nextGroupTarget = toBeGrouped;
                }
                while (nextGroupTarget.Any())
                {
                    var currentLayer = GetFacadeNodes(nextGroupTarget);
                    nextGroupTarget = currentLayer.SiblingDependencies().ToList();
                    //Get the next layer to check if any of the dependencies are unique to a node of the current layer
                    var nextLayer = GetFacadeNodes(nextGroupTarget);
                    var uniqueDependencies =
                        nextLayer.Where(x => !currentLayer.All(n => n.SiblingDependencies.Contains(x)))
                            .ToImmutableHashSet();

                    //If there are unique dependencies, vertical layers are created to separate the unique dependency from layers that dont depend on it
                    if (uniqueDependencies.Any())
                    {
                        var dependencyGroups = FindDependencyGroups(currentLayer, nextLayer);
                        toBeGrouped.RemoveRange(currentLayer);
                        foreach (var dependencyGroup in dependencyGroups)
                        {
                            currentLayer.RemoveRange(dependencyGroup.Referencers.ToList());
                            var dependants = dependencyGroup.Dependants.ToList();
                            nextGroupTarget.RemoveRange(dependants);
                            toBeGrouped.RemoveRange(dependants);
                            var depNode = CreateHorizontalLayer(dependants);
                            var referenceNode = CreateHorizontalLayer(dependencyGroup.Referencers);
                            var newList = new List<Node> {depNode, referenceNode};
                            //Needs to be only those that are unique dependencies
                            var nestedDependencies = dependants.SiblingDependencies().ToList();
                            //Get the other groups
                            var otherGroups = dependencyGroups.Except(dependencyGroup);
                            var nodesInOtherGroups = otherGroups.SelectMany(x => x.Dependants.Union(x.Referencers));
                            var otherNodes =
                                toBeGrouped.Union(nodesInOtherGroups).Union(currentLayer).Except(nestedDependencies);
                            var nonUniqueDependencies = otherNodes.SiblingDependencies();
                            //Remove nodes that are dependency for a node in other vertical layer or left in childlist (except for those in depNodeDep)
                            nestedDependencies.RemoveRange(
                                nestedDependencies.Where(x => nonUniqueDependencies.Contains(x)).ToList());

                            foreach (var depNodeDependency in nestedDependencies)
                            {
                                depNodeDependency.SiblingDependencies.RemoveWhere(x => !nestedDependencies.Contains(x));
                            }
                            //Should be any of the verticals
                            nestedDependencies.RemoveRange(
                                nestedDependencies.Where(x => toBeGrouped.Any(y => y.DependsOn(x))).ToList());
                            if (nestedDependencies.Any())
                            {
                                toBeGrouped.RemoveRange(nestedDependencies);
                                nextGroupTarget.RemoveRange(nestedDependencies);
                                var dependenciesOfNestedNodes = nestedDependencies.SiblingDependencies().ToList();
                                if (!dependenciesOfNestedNodes.Any())
                                    nestedDependencies = new List<Node> {CreateHorizontalLayer(nestedDependencies)};
                                else
                                {
                                    //Remove all nodes from this levels stack that should be added by the recursive call
                                    toBeGrouped.RemoveRange(dependenciesOfNestedNodes);
                                    nextGroupTarget.RemoveRange(dependenciesOfNestedNodes);
                                    nestedDependencies = GroupNodes(nestedDependencies).ToList();
                                }
                                newList.InsertRange(0, nestedDependencies);
                            }
                            currentLayer.Add(new VerticalSiblingHolderNode(newList));
                        }
                        nextGroupTarget = toBeGrouped;
                    }
                    else
                    {
                        toBeGrouped.RemoveRange(currentLayer);
                    }
                    groupedNodes.Add(CreateHorizontalLayer(currentLayer));
                }
            }
            //TODO: handle this or just skip?
            //if (unreferenced.Any())
            //    newChildOrder.Add(CreateHorizontalLayer(unreferenced));

            groupedNodes.Reverse();
            return groupedNodes;
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