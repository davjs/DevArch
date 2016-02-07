using System;
using System.Collections.Generic;
using System.Linq;
using Logic.SemanticTree;

namespace Logic.Filtering
{
    public static class CircularReferenceFinder
    {
        public static void FindCircularReferences(ISet<Node> childList)
        {
            FindCircularReferences(childList, childList);
        }

        private static void FindCircularReferences(ISet<Node> childList1, ISet<Node> childList2)
        {
            while (true)
            {
                var circularRefs = new List<Tuple<Node, Node>>();
                foreach (var node in childList1)
                {
                    foreach (var node2 in from node2 in childList2.Where(n => n != node) where node.SiblingDependencies.Contains(node2) where node2.SiblingDependencies.Contains(node) where circularRefs.All(x => x.Item1 != node2 && x.Item2 != node) select node2)
                    {
                        circularRefs.Add(new Tuple<Node, Node>(node, node2));
                    }
                }
                var circularDependencyHolders = new HashSet<Node>();
                //May need to check for circular references with the newly created CircularDependencyHolderNode?
                foreach (var circularRef in circularRefs)
                {
                    var circularDependencyHolderNode = new CircularDependencyHolderNode(new List<Node> {circularRef.Item1, circularRef.Item2});
                    circularDependencyHolderNode.SiblingDependencies.UnionWith(circularRef.Item1.SiblingDependencies.Union(circularRef.Item2.SiblingDependencies));
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
                    childList1 = circularDependencyHolders;
                else
                    break;
            }
        }
    }
}