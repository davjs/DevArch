using System;
using System.IO;
using System.Linq;
using Logic.Building.SemanticTree;
using Logic.Integration;
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
            ClassTreeBuilder.AddClassesToTree(tree);
        }


        public static Tree AnalyseDocument(AdvancedSolution solution, string documentName)
        {
            var tree = new Tree();
            var projectName = GetRootFolder(documentName);
            var fname = Path.GetFileName(documentName);
            ProjectTreeBuilder.AddProjectToTree(solution.RoslynSolution, ref tree, projectName);
            ClassTreeBuilder.AddClassesToTree(tree, fname);
            tree = tree.DescendantNodes().First(x => x is ClassNode).Parent;
            return tree;
        }


        public static Tree AnalyseProject(AdvancedSolution solution, string projectName)
        {
            var tree = new Tree();
            ProjectTreeBuilder.AddProjectToTree(solution.RoslynSolution, ref tree, projectName);
            ClassTreeBuilder.AddClassesToTree(tree);
            return tree;
        }
        
        public static Tree AnalyseNamespace(AdvancedSolution solution, string name)
        {
            var projectName = GetRootFolder(name);
            var tree = AnalyseProject(solution, projectName);
            var names = name.Split('\\').ToList();
            names.RemoveAt(0);

            while (names.Any())
            {
                var nextName = names.First();
                var nextTree = tree.DescendantNodes().WithName(nextName);
                if (nextTree == null)
                    return tree;
                tree = nextTree;
                names.RemoveAt(0);
            }
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


    internal class LayerViolationException : Exception
    {
    }
}