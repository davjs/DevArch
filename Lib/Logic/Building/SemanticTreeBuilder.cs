using System;
using System.IO;
using System.Linq;
using Logic.Integration;
using Logic.SemanticTree;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Building
{
    public static class SemanticTreeBuilder
    {
        public static SolutionNode AnalyseSolution(AdvancedSolution solution)
        {
            var tree = new SolutionNode(solution);
            ProjectTreeBuilder.AddSolutionFoldersToTree(solution.DteProjects,ref tree);
            AddAllItemsInSolutionToTree(solution.RoslynSolution, ref tree);
            return tree;
        }

        public static void AddAllItemsInSolutionToTree(Solution solution, ref SolutionNode tree)
        {
            ProjectTreeBuilder.AddProjectsToTree(solution, ref tree);
            ClassTreeBuilder.AddClassesToTree(tree);
        }


        public static Node AnalyseDocument(AdvancedSolution solution, string documentName)
        {
            var tree = new SolutionNode(solution);
            var projectName = GetRootFolder(documentName);
            var fname = Path.GetFileName(documentName);
            ProjectTreeBuilder.AddProjectToTree(solution.RoslynSolution, ref tree, projectName);
            ClassTreeBuilder.AddClassesToTree(tree, fname);
            var nSpace = tree.DescendantNodes().First(x => x is ClassNode).Parent;
            return nSpace;
        }


        public static SolutionNode AnalyseProject(AdvancedSolution solution, string projectName)
        {
            var tree = new SolutionNode(solution);
            ProjectTreeBuilder.AddProjectToTree(solution.RoslynSolution, ref tree, projectName);
            ClassTreeBuilder.AddClassesToTree(tree);
            return tree;
        }
        
        public static Node AnalyseNamespace(AdvancedSolution solution, string name)
        {
            var projectName = GetRootFolder(name);
            var tree = AnalyseProject(solution, projectName) as Node;
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

        public static Node AnalyseClass(AdvancedSolution solution, string name)
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
}