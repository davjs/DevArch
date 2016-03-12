using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Logic.Integration;
using Logic.SemanticTree;
using MoreLinq;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Building
{
    public static class SemanticTreeBuilder
    {
        public static SolutionNode AnalyseSolution(DevArchSolution solution)
        {
            var tree = solution.SolutionTree;
            AddAllItemsInSolutionToTree(solution.RoslynSolution, ref tree);
            return tree;
        }

        public static void AddAllItemsInSolutionToTree(Solution solution, ref SolutionNode tree)
        {
            ProjectTreeBuilder.AddProjectsToTree(solution, ref tree);
            if (!tree.Childs.Any())
                throw new NoCsharpProjectsFoundException();
            ClassTreeBuilder.AddClassesInProjectsToTree(tree);
        }


        public static Node FindDocument(SolutionNode solution, string path)
        {
            var projectName = GetRootFolder(path);
            var documentName = Path.GetFileName(path);
            var tree = FindProject(solution, projectName);
            var docsMatching = tree.Documents.Where(x => x.Name == documentName).ToList();
            if (!docsMatching.Any())
                throw new Exception("Unable to find document: " + path);
            if(docsMatching.Count > 1)
                throw new NotImplementedException($"Got {docsMatching.Count} matching documents, dont know which one to pick");
            var doc = docsMatching.First();
            ///////////////////////////////////////////////////////////////////////
            return ClassTreeBuilder.AddClassesInDocument(doc);
        }


        public static ProjectNode FindProject(SolutionNode solution, string projectName)
        {
            var projects = solution.DescendantNodes().OfType<ProjectNode>().ToList();
            var proj = projects.WithName(projectName);
            if(proj == null)
                throw new Exception($"Could not find project {projectName} among the following: {projects.Select(x => x.Name).ToDelimitedString()}");
            return proj;
        }
        
        public static Node FindNamespace(SolutionNode solution, string name)
        {
            var projectName = GetRootFolder(name);
            var tree = FindProject(solution, projectName) as Node;
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

        public static Node FindClass(SolutionNode solution, string name)
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

    public class NoCsharpProjectsFoundException : Exception
    {
    }
}