using System;
using System.IO;
using System.Linq;
using EnvDTE;
using Logic.Building.SemanticTree;
using Microsoft.CodeAnalysis.MSBuild;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Building
{
    public static class SemanticTreeBuilder
    {
        public static Tree AnalyseSolution(AdvancedSolution solution)
        {
            var tree = ProjectTreeBuilder.AddSolutionFoldersToTree(solution.DteProjects);
            AddAllItemsInSolutionToTree(solution.RoslynSolution, ref tree);
            return tree;
        }

        public static void AddAllItemsInSolutionToTree(Solution solution, ref Tree tree)
        {
            ProjectTreeBuilder.AddProjectsToTree(solution, ref tree);
            ClassTreeBuilder.AddClassesToTree(solution, tree);
            tree = DependencyBuilder.BuildDependenciesFromReferences(tree);
        }


        public static Tree AnalyseDocument(AdvancedSolution solution, string name)
        {
            var tree = new Tree();
            var pName = GetRootFolder(name);
            var fname = Path.GetFileName(name);
            ProjectTreeBuilder.AddProjectToTree(solution.RoslynSolution, ref tree, pName);
            ClassTreeBuilder.AddClassesToTree(solution.RoslynSolution, tree, fname);
            tree = tree.DescendantNodes().First(x => x is ClassNode).Parent;
            tree = DependencyBuilder.BuildDependenciesFromReferences(tree);
            return tree;
        }

        public static Tree AnalyseClass(AdvancedSolution solution, string name)
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

        


    }

    public class AdvancedSolution
    {
        public readonly Solution RoslynSolution;
        public readonly Projects DteProjects;
        public readonly string FullName;
        public AdvancedSolution(_DTE dte)
        {
            var build = MSBuildWorkspace.Create();
            var dteSolution = dte.Solution;
            FullName = GetSolutionName(dteSolution);
            RoslynSolution = GetSolution(build, FullName);
            DteProjects = dteSolution.Projects;
        }

        private static string GetSolutionName(_Solution solution)
        {
            string name = null;
            while (name == null)
            {
                try
                {
                    name = solution.FullName;
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