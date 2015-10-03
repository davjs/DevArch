using System;
using System.Collections.Generic;
using System.Linq;
using Analysis.Building;
using Analysis.SemanticTree;
using EnvDTE;
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
            AnalyzeSolutionToTree(solution, ref tree);
            return tree;
        }

        public static void AnalyzeSolutionToTree(Solution solution, ref Tree tree)
        {
            ProjectTreeBuilder.AddProjectsToTree(solution, ref tree);
            ClassTreeBuilder.AddClassesToTree(solution, tree);
            tree = SemanticTreeBuilder.BuildDependenciesFromReferences(tree);
            tree = RemoveTests(tree);
            tree = RemoveSinglePaths(tree);
            tree.UpdateChildren(tree.Childs.Select(FindSiblingDependencies).ToList());
            //EROR HAPPENS HERE
            tree.UpdateChildren(SiblingReordrer.OrderChildsBySiblingsDependencies(tree.Childs).ToList());
            tree = FindSiblingPatterns(tree);
        }

        private static Tree FindSiblingPatterns(Tree tree)
        {
            if (!tree.Childs.Any())
                return tree;
            var baseClassPattern = PatternFinder.FindBaseClassPattern(tree.Childs);
            if (baseClassPattern != null)
            {
                tree.UpdateChildren(new List<Node>());
                tree.AddChild(new Node(baseClassPattern + "s"));
            }
            else
            {
                var namingPattern = PatternFinder.FindNamingPattern(tree.Childs.Select(x => x.Name));
                if (namingPattern != null)
                {
                    tree.UpdateChildren(new List<Node>());
                    tree.AddChild(new Node(namingPattern + "s"));
                }
            }
            tree.UpdateChildren(tree.Childs.Select(FindSiblingPatterns).Cast<Node>());
            return tree;
        }

        private static Tree RemoveTests(Tree tree)
        {
            tree.UpdateChildren(tree.Childs.Select(RemoveTests).Cast<Node>());
            tree.UpdateChildren(
                tree.Childs.Where(x => !x.Name.EndsWith("test", StringComparison.InvariantCultureIgnoreCase)).ToList());
            tree.UpdateChildren(
                tree.Childs.Where(x => !x.Name.EndsWith("tests", StringComparison.InvariantCultureIgnoreCase)).ToList());
            return tree;
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