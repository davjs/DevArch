using System.Collections.Generic;
using System.Linq;

namespace Analysis.SemanticTree
{
    public static class NodeExtensions
    {
        public static T WithName<T>(this IEnumerable<T> nodeList,string name) where T : Node
        {
            return nodeList.FirstOrDefault(x => x.Name == name);
        }

        public static IEnumerable<Node> Dependencies(this IEnumerable<Node> nodeList)
        {
            return nodeList.SelectMany(x => x.Dependencies);
        }
        
        public static IEnumerable<Node> AllSubDependencies(this Node node)
        {
            return node.Dependencies.Concat(node.Childs.SelectMany(AllSubDependencies)).Distinct();
        }
        public static IEnumerable<Node> SiblingDependencies(this IEnumerable<Node> nodeList)
        {
            return nodeList.SelectMany(x => x.SiblingDependencies);
        }
        public static IEnumerable<Node> DependantOfNode(this IEnumerable<Node> nodeList ,Node node)
        {
            return nodeList.Where(x => x.SiblingDependencies.Contains(node)).ToList();
        }

        public static IEnumerable<Node> DescendantNodes(this Tree tree)
        {
            foreach (var child in tree.Childs)
            {
                yield return child;
                foreach (var descendantsOfChild in child.DescendantNodes())
                {
                    yield return descendantsOfChild;
                }
            }
        }
    }
}