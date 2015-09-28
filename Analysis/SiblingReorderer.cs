using System.Collections.Generic;
using System.Linq;

namespace Analysis
{
    public static class SiblingReorderer
    {
        public static IEnumerable<Node> OrderChildsBySiblingsDependencies(IReadOnlyList<Node> childs)
        {
            foreach (var child in childs)
            {
                if(child.Childs.Any())
                    child.UpdateChildren(SiblingReorderer.OrderChildsBySiblingsDependencies(child.Childs));
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

                newChildOrder.Add(node); // Refactor out
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
                    if(!SiblingReordrer.TryGroupSiblingsLinearly(node,dependantOfNode,ref newChildOrder))
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
    }
}