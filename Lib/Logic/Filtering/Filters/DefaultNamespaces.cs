using System;
using System.Linq;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class DefaultNamespaces : Filter
    {
        public DefaultNamespaces(bool i) : base(i, Apply)
        {
        }

        private new static void Apply(Node tree)
        {
            var projects = tree.Projects();
            var withDefaultNamespaces = projects.Where(p => p.Childs.Count() == 1 && p.Childs.First().Name == p.Name);
            foreach (var projectNode in withDefaultNamespaces)
            {
                var childNode = projectNode.Childs.First();
                TakeOver(projectNode, childNode);
            }
        }

        private static void TakeOver(Node parentNode, Node childNode)
        {
            parentNode.Orientation = childNode.Orientation;
            parentNode.SetChildren(childNode.Childs);
        }
    }
}