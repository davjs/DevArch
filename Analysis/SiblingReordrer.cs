using System.Collections.Generic;
using System.Linq;
using Analysis.SemanticTree;

namespace Analysis
{
    public static class SiblingReordrer
    {
        public static IEnumerable<Node> OrderChildsBySiblingsDependencies(IReadOnlyList<Node> childs)
        {
            foreach (var child in childs)
            {
                if(child.Childs.Any())
                    child.UpdateChildren(OrderChildsBySiblingsDependencies(child.Childs));
            }

            if (!childs.SiblingDependencies().Any())
            {
                if(childs.Any() && childs.First().Parent != null)
                    childs.First().Parent.Horizontal = true;
                return childs;
            }
            var nodesWithoutDependency = childs.Where(c => !c.SiblingDependencies.Any()).ToList();
            if (nodesWithoutDependency.Count == 0)
                throw new LayerViolationException();
            var oldChildList = childs.ToList();
            var newChildOrder = new List<Node>();
            foreach (var node in nodesWithoutDependency)
            {
                RegroupSiblingNodes(node, oldChildList, ref newChildOrder);
            }
            return newChildOrder;
        }

        public static void RegroupSiblingNodes(Node startingNode, List<Node> oldChildList,ref List<Node> newChildOrder)
        {
            var previousNode = startingNode;
            var node = startingNode;
            while (node != null)
            {
                var dependantOfNode = oldChildList.DependantOfNode(previousNode).ToList();
                if (!dependantOfNode.Any())
                    break;

                newChildOrder.Add(node);
                oldChildList.Remove(node);
                if (dependantOfNode.Count == 1)
                {
                    previousNode = node;
                    node = dependantOfNode.First();
                    newChildOrder.Add(node);
                    oldChildList.Remove(node);
                }
                else
                {
                    if(!TryGroupSiblingsLinearly(node,dependantOfNode,ref newChildOrder))
                    { 
                        newChildOrder.Add(new SiblingHolderNode(dependantOfNode));
                        oldChildList.RemoveAll(x => dependantOfNode.Contains(x));
                        var dependantOfNodes = dependantOfNode.SelectMany(oldChildList.DependantOfNode).ToList();
                        if (!dependantOfNodes.Any())
                            node = null;
                    }
                }
            }
        }

        public static bool TryGroupSiblingsLinearly(Node startNode,IList<Node> nodes, ref List<Node> newChildOrder)
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