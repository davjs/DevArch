using System;
using System.IO;
using System.Linq;
using EnvDTE;
using Logic.Analysis.Building;
using Logic.Analysis.SemanticTree;
using Microsoft.CodeAnalysis.MSBuild;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Analysis
{
    public static class Analyser
    {
        public static Tree AnalyseSolution(_DTE dte, Projects projects)
        {
            var build = MSBuildWorkspace.Create();
            var name = GetSolutionName(dte);
            var solution = GetSolution(build, name);

            var tree = ProjectTreeBuilder.AddSolutionFoldersToTree(projects);
            AddAllItemsInSolutionToTree(solution, ref tree);
            return tree;
        }

        public static void AddAllItemsInSolutionToTree(Solution solution, ref Tree tree)
        {
            ProjectTreeBuilder.AddProjectsToTree(solution, ref tree);
            ClassTreeBuilder.AddClassesToTree(solution, tree);
            tree = SemanticTreeBuilder.BuildDependenciesFromReferences(tree);
        }


        public static Tree AnalyseDocument(_DTE dte, string name)
        {
            var build = MSBuildWorkspace.Create();
            var sname = GetSolutionName(dte);
            var solution = GetSolution(build, sname);
            var tree = new Tree();
            var pName = GetRootFolder(name);
            var fname = Path.GetFileName(name);
            ProjectTreeBuilder.AddProjectToTree(solution, ref tree, pName);
            ClassTreeBuilder.AddClassesToTree(solution, tree, fname);
            tree = SemanticTreeBuilder.BuildDependenciesFromReferences(tree);
            tree = tree.DescendantNodes().First(x => x is ClassNode).Parent;
            return tree;
        }

        public static Tree AnalyseClass(_DTE dte, string name)
        {
            throw new NotImplementedException();

        }

        static string GetRootFolder(string path)
        {
            while (true)
            {
                var temp = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(temp))
                    break;
                path = temp;
            }
            return path;
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

        private static Solution GetSolution(MSBuildWorkspace build, string name)
        {
            Solution solution = null;
            while (solution == null)
            {
                try
                {
                    solution = build.OpenSolutionAsync(name).Result;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return solution;
        }


    }

    internal class LayerViolationException : Exception
    {
    }
}