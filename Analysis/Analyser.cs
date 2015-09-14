using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis.MSBuild;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Analysis
{
    public static class Analyser
    {
        public static Tree AnalyseEnviroment(DTE dte2)
        {
            var build = MSBuildWorkspace.Create();
            var name = GetSolutionName(dte2);
            var solution = build.OpenSolutionAsync(name).Result;
            return AnalyzeSolution(solution);
        }

        public static Tree AnalyzeSolution(Solution solution)
        {
            var tree = BuildTreeFromSolution(solution);
            tree = SemanticTreeBuilder.BuildDependenciesFromReferences(tree);
            tree = RemoveSinglePaths(tree);
            tree.UpdateChildren(tree.Childs.Select(FindSiblingDependencies).ToList());
            tree.UpdateChildren(OrderChildsBySiblingsDependencies(tree.Childs).ToList());
            return tree;
        }

        public static IEnumerable<Node> OrderChildsBySiblingsDependencies(IReadOnlyList<Node> childs)
        {
            foreach (var child in childs)
            {
                child.UpdateChildren(OrderChildsBySiblingsDependencies(child.Childs));
            }

            if (!childs.SiblingDependencies().Any()) return childs;
            var nodesWithoutDependency = childs.Where(c => !c.SiblingDependencies.Any()).ToList();
            if (nodesWithoutDependency.Count == 0)
                throw new LayerViolationException();
            //TODO: THROW ERROR WHEN MORE THAN ONE??
            var startingNode = nodesWithoutDependency.First();
            var newChildOrder = new List<Node>();
            var previousNode = startingNode;
            for (var node = startingNode; node != null; node = childs.FirstOrDefault(x => x.SiblingDependencies.Contains(previousNode)))
            {
                newChildOrder.Add(node);
                previousNode = node;
            }
            return newChildOrder;
        }

        public static Tree BuildTreeFromSolution(Solution solution)
        {
            var projects = solution.Projects.ToList();
            var tree = new Tree();
            if (projects.Count <= 0) return tree;
            var projectTrees = new List<Node>();
            foreach (var project in projects)
            {
                var projectNode = new Node(project.Name);
                var documents = project.Documents.ToList();
                if (documents.Any())
                {
                    var semanticModels = documents.Select(d => d.GetSemanticModelAsync().Result);
                    var classes = SemanticModelWalker.GetClassesInModels(semanticModels, solution);
                    if (!classes.Any())
                        throw new Exception("No classes found");
                    var classnodes = SemanticTreeBuilder.BuildTreeFromClasses(classes);
                    projectNode.AddChilds(classnodes);
                }
                projectTrees.Add(projectNode);
            }
            tree.AddChilds(projectTrees);
            return tree;
        }

        public static Node FindSiblingDependencies(Node node)
        {
            node.UpdateChildren(node.Childs.Select(FindSiblingDependencies).ToList());
            if (node.Parent == null) return node;
            foreach (
                var sibling in
                    node.Dependencies.Concat(node.Childs.Dependencies())
                        .Select(dependency => AncestorIsSibling(node.Parent, dependency))
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
                if (p.Parent as Node == parent)
                    return p;
            }
            return null;
        }

        private static Tree RemoveSinglePaths(Tree tree)
        {
            tree.UpdateChildren(tree.Childs.Select(RemoveSinglePaths).Cast<Node>().ToList());
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