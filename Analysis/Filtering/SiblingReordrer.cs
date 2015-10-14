using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Analysis.SemanticTree;

namespace Logic.Analysis
{
    public static class SiblingReordrer
    {
        public static IEnumerable<Node> OrderChildsBySiblingsDependencies(IReadOnlyList<Node> childs)
        {
            foreach (var child in childs)
            {
                if (child.Childs.Any())
                    child.UpdateChildren(OrderChildsBySiblingsDependencies(child.Childs));
            }

            if (!childs.SiblingDependencies().Any())
            {
                if (childs.Any() && childs.First().Parent != null)
                    childs.First().Parent.Horizontal = true;
                return childs;
            }

            var oldChildList = childs.ToList();
            FindCircularReferences(ref oldChildList);
            var nodesWithoutDependency = oldChildList.Where(c => !c.SiblingDependencies.Any()).ToList();
            var newChildOrder = new List<Node>();
            if (nodesWithoutDependency.Count == 0)
                throw new LayerViolationException();
            RegroupSiblingNodes(new List<Node>(), oldChildList, ref newChildOrder);
            return newChildOrder;
        }

        public static void RegroupSiblingNodes(List<Node> nodesWithoutDependency, List<Node> oldChildList,
            ref List<Node> newChildOrder)
        {
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
                newChildOrder.Add(nodesWithoutDependency.Count == 1
                    ? nodesWithoutDependency.First()
                    : new SiblingHolderNode(nodesWithoutDependency));
                foreach (var node in nodesWithoutDependency)
                {
                    oldChildList.Remove(node);
                }
            }
        }

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

        public static bool TryGroupSiblingsLinearly(Node startNode, IList<Node> nodes, ref List<Node> newChildOrder)
        {
            while (true)
            {
                foreach (var node in nodes)
                {
                    node.SiblingDependencies.Remove(startNode);
                }
                var nodesWithoutSiblingDependencies = nodes.Where(x => !x.SiblingDependencies.Any()).ToList();
                if (nodesWithoutSiblingDependencies.Count != 1) return false;
                newChildOrder.Add(nodesWithoutSiblingDependencies.First());
                startNode = nodesWithoutSiblingDependencies.First();
                nodes.Remove(startNode);
                if (!nodes.Any())
                    return true;
            }
        }
    }
}