using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis.MSBuild;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Analysis
{
    public static class Analyser
    {
        public static Tree AnalyseEnviroment(DTE dte)
        {
            var build = MSBuildWorkspace.Create();
            var name = GetSolutionName(dte);
            var solution = build.OpenSolutionAsync(name).Result;
            var tree = ProjectTreeBuilder.GetSolutionFoldersTree(dte);
            AnalyzeSolutionToTree(solution,ref tree);
            return tree;
        }

        public static void AnalyzeSolutionToTree(Solution solution,ref Tree tree)
        {
            ProjectTreeBuilder.AddProjectsToTree(solution,ref tree);
            ClassTreeBuilder.AddClassesToTree(solution, tree);
            tree = SemanticTreeBuilder.BuildDependenciesFromReferences(tree);
            tree = RemoveSinglePaths(tree);
            tree.UpdateChildren(tree.Childs.Select(FindSiblingDependencies).ToList());
            tree.UpdateChildren(OrderChildsBySiblingsDependencies(tree.Childs).ToList());
            tree = RemoveTests(tree);
        }

        private static Tree RemoveTests(Tree tree)
        {
            tree.UpdateChildren(tree.Childs.Select(RemoveTests).Cast<Node>());
            tree.UpdateChildren(tree.Childs.Where(x => !x.Name.EndsWith("test",StringComparison.InvariantCultureIgnoreCase)).ToList());
            tree.UpdateChildren(tree.Childs.Where(x => !x.Name.EndsWith("tests", StringComparison.InvariantCultureIgnoreCase)).ToList());
            return tree;
        }


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

        private static void RegroupSiblingNodes(Node startingNode, List<Node> oldChildList,ref List<Node> newChildOrder)
        {
            var previousNode = startingNode;
            var node = startingNode;
            while (node != null)
            {
                var dependantOfNode = oldChildList.DependantOfNode(previousNode);
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
                    newChildOrder.Add(new SiblingHolderNode(dependantOfNode));
                    oldChildList.RemoveAll(x => dependantOfNode.Contains(x));
                    var dependantOfNodes = dependantOfNode.SelectMany(oldChildList.DependantOfNode).ToList();
                    if (!dependantOfNodes.Any())
                        node = null;
                }
            }
        }

        public static Node FindSiblingDependencies(Node node)
        {
            node.UpdateChildren(node.Childs.Select(FindSiblingDependencies).ToList());
            if (node.Parent == null) return node;
            var allSubDependencies = node.AllSubDependencies().ToList();
            var dependenciesWhereAncestorsAreSiblings = allSubDependencies
                .Select(dependency => AncestorIsSibling(node.Parent, dependency)).ToList();
            foreach (
                var sibling in
                    dependenciesWhereAncestorsAreSiblings
                        .Where(sibling => sibling != null && sibling != node))
            {
                node.SiblingDependencies.Add(sibling);
            }

            return node;
        }

        private static Node AncestorIsSibling(Tree parent, Node dependency)
        {
            for (var p = dependency; p != null; p = p.Parent as Node)
            {
                if (p.Parent.Id == parent.Id)
                    return p;
            }
            return null;
        }

        public static Tree RemoveSinglePaths(Tree tree)
        {
            tree.UpdateChildren(tree.Childs.Select(Analyser.RemoveSinglePaths).Cast<Node>().ToList());
            return tree.Childs.Count == 1 ? tree.Childs.First() : tree;
        }

        private static string GetSolutionName(_DTE dte2)
        {
            string name = null;
            while (name == null)
            {
                try
                {
                    name = dte2.Solution.FullName;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return name;
        }
    }

    internal class LayerViolationException : Exception
    {
    }
}