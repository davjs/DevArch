using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Logic.Filtering;
using MoreLinq;

namespace Logic.SemanticTree
{
    public static class NodeExtensions
    {
        public static T WithName<T>(this IEnumerable<T> nodeList,string name) where T : Node
        {
            return nodeList.FirstOrDefault(x => x.Name == name);
        }
        
        public static IEnumerable<Node> AllSubDependencies(this Node node)
        {
            return node.Dependencies.Concat(node.Childs.SelectMany(AllSubDependencies)).Distinct();
        }
        public static bool HasChildren(this Node node)
        {
            return node.Childs.Any();
        }
        public static HashSet<Node> SiblingDependencies(this IEnumerable<Node> nodeList)
        {
            return nodeList.SelectMany(x => x.SiblingDependencies).Distinct().ToHashSet();
        }
        public static bool DependsOn(this Node node,Node dependency)
        {
            return node.SiblingDependencies.Contains(dependency);
        }

        public static bool IndirectlyDependsOn(this Node node,Node dependency)
        {
            return node.IndirectSiblingDependencies().Contains(dependency);
        }

        /*public static int TotalReferences(this IEnumerable<Node> nodes)
        {
            return nodes.Select(n => n.References).Distinct().Count();
        }*/

        public static IEnumerable<ProjectNode> Projects(this Node tree)
        {
            var projects = new List<ProjectNode>();
            foreach (var child in tree.Childs)
            {
                if(child is ProjectNode)
                    projects.Add(child as ProjectNode);
                if (child is ClassNode)
                    return projects;
                projects.AddRange(child.Projects());
            }
            return projects;
        }

        /*public static IEnumerable<ProjectNode> Projects(this Tree tree)
        {
            foreach (var child in tree.Childs)
            {
                if(child is ProjectNode)
                    yield return child as ProjectNode;
                if (child is ClassNode)
                    yield break;
                foreach (var descendantsOfChild in child.Projects())
                {
                    yield return descendantsOfChild;
                }
            }
        } */

        public static IEnumerable<Node> DescendantNodes(this Node tree)
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

        /*public static IEnumerable<Node> IndirectSiblingDependencies(this Node tree)
        {
            foreach (var dependency in tree.SiblingDependencies)
            {
                yield return dependency;
                foreach (var dependenciesOfDependency in IndirectSiblingDependencies(dependency))
                {
                    yield return dependenciesOfDependency;
                }
            }
        }*/
    }
}