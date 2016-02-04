using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Logic.Common;
using Logic.Filtering;
using Logic.SemanticTree;

namespace Logic.Ordering
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
                    childs.First().Parent.Orientation = OrientationKind.Horizontal;
                return childs;
            }

            var oldChildList = childs.ToList();
            var newChildOrder = GroupNodes(oldChildList);
            return newChildOrder;
        }

        private static IEnumerable<Node> GroupNodes(List<Node> oldChildList)
        {
            CircularReferenceFinder.FindCircularReferences(ref oldChildList);
            return LayOutSiblingNodes(oldChildList);
        }

        public static List<Node> GetFacadeNodes(List<Node> targets)
        {
            var allDependencies = targets.SiblingDependencies().ToList();
            return targets.Where(n => !allDependencies.Contains(n)).ToList();
        }

        public static List<Node> LayOutSiblingNodes(List<Node> toBeGrouped)
        {
            if (toBeGrouped.Count <= 1)
                return toBeGrouped;

            var groupedNodes = new List<Node>();
            var nextGroupTarget = toBeGrouped;

            //Remove nodes that does not depend on anything and is never referenced
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

                    //Check if any nodes that have not been added yet has dependencies on the unique ones, in this case they arent really unique
                    var leftForNextBatch = toBeGrouped.Except(currentLayer.Union(nextLayer));
                    nextLayer.RemoveAll(x => leftForNextBatch.SiblingDependencies().Contains(x));

                    var uniqueDependencies =
                        nextLayer.Where(x => !currentLayer.All(n => n.SiblingDependencies.Contains(x))).Distinct().ToList();
                    

                    //If there are unique dependencies, vertical layers are created to separate the unique dependency from layers that dont depend on it
                    if (uniqueDependencies.Any())
                    {
                        var groupsToCreate = FindDependencyGroups(currentLayer, nextLayer);
                        var toBeShared = new HashSet<Node>();
                        toBeGrouped.RemoveRange(currentLayer);
                        foreach (var dependencyGroup in groupsToCreate)
                        {
                            currentLayer.RemoveRange(dependencyGroup.Referencers.ToList());
                            var dependants = dependencyGroup.Dependants.ToList();
                            nextGroupTarget.RemoveRange(dependants);
                            toBeGrouped.RemoveRange(dependants);
                            // Add dependant to the vertical layer
                            var depNode = CreateHorizontalLayer(dependants);
                            // Add references to the vertical layer
                            var referenceNode = CreateHorizontalLayer(dependencyGroup.Referencers);
                            var newList = new List<Node> {depNode, referenceNode};
                            //Get ALL the possible candidates for the vertical layer
                            var verticalCandidates = dependants.SelectMany(x => x.IndirectSiblingDependencies()).Distinct().ToList();

                            //Get all the nodes in this current call depth
                            var otherGroups = groupsToCreate.Except(dependencyGroup);
                            var nodesInOtherGroups = otherGroups.
                                SelectMany(x => x.Dependants.Union(x.Referencers));
                            var otherNodes =
                                toBeGrouped.Union(currentLayer).Union(nodesInOtherGroups).Except(verticalCandidates);

                            //If any of the other nodes depends on the vertical candidate the candidate is removed and will be placed in a later iteration of this call (it is still left in toBeGrouped)
                            foreach (var nestedNode in verticalCandidates.ToList().
                                Where(nestedNode => toBeShared.Contains(nestedNode) || 
                                otherNodes.Any(x => x.IndirectlyDependsOn(nestedNode))))
                            {
                                verticalCandidates.Remove(nestedNode);
                                verticalCandidates.ForEach(x => x.SiblingDependencies.Remove(nestedNode));
                                toBeShared.Add(nestedNode);
                            }

                            if (verticalCandidates.Any())
                            {
                                toBeGrouped.RemoveRange(verticalCandidates);
                                nextGroupTarget.RemoveRange(verticalCandidates);
                                var dependenciesOfNestedNodes = verticalCandidates.SiblingDependencies().ToList();
                                if (!dependenciesOfNestedNodes.Any())
                                    verticalCandidates = new List<Node> {CreateHorizontalLayer(verticalCandidates)};
                                else
                                {
                                    //Remove all nodes from this levels stack that should be added by the recursive call
                                    toBeGrouped.RemoveRange(dependenciesOfNestedNodes);
                                    nextGroupTarget.RemoveRange(dependenciesOfNestedNodes);
                                    verticalCandidates = GroupNodes(verticalCandidates).ToList();
                                }
                                newList.InsertRange(0, verticalCandidates);
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
            //TODO: Add if only one layer
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
}