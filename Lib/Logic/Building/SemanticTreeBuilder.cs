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
            //tree = DependencyBuilder.BuildDependenciesFromReferences(tree);
        }


        public static Tree AnalyseDocument(AdvancedSolution solution, string name)
        {
            var tree = new Tree();
            var pName = GetRootFolder(name);
            var fname = Path.GetFileName(name);
            ProjectTreeBuilder.AddProjectToTree(solution.RoslynSolution, ref tree, pName);
            ClassTreeBuilder.AddClassesToTree(solution.RoslynSolution, tree, fname);
            tree = tree.DescendantNodes().First(x => x is ClassNode).Parent;
            //tree = DependencyBuilder.BuildDependenciesFromReferences(tree);
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
            FullName = KeepTrying.ToGet(() => dteSolution.FullName);
            RoslynSolution = KeepTrying.ToGet(() => build.OpenSolutionAsync(FullName).Result);
            DteProjects = KeepTrying.ToGet(() =>dteSolution.Projects);
        }
    }

    static class KeepTrying
    {
        static int _totalRetries = 0;
        public delegate void Del();
        const int Maxtries = 5;

        public static T ToGet<T>(Func<T> function, bool thrw = true) where T : class
        {
            T result = null;
            var failed = true;
            var tries = 0;
            while (failed && result == null)
            {
                tries++;
                try
                {
                    result = function();
                    failed = false;
                }
                catch (Exception e)
                {
                    if (tries < Maxtries) continue;
                    if (thrw)
                    {
                        System.Diagnostics.Debug.WriteLine("EXCEPTION ROOF");
                        System.Diagnostics.Debug.WriteLine(e);
                        throw;
                    }
                    if (_totalRetries < Maxtries)
                    {
                        _totalRetries++;
                        tries = 0;
                    }
                    else
                    {
                        thrw = true;
                    }
                }
            }
            return result;
        } 

        public static void UntilNotFailing(Action function, bool thrw = true)
        {
            bool failed = true;
            int tries = 0;
            while (failed)
            {
                tries++;
                try
                {
                    function();
                    failed = false;
                }
                catch (System.Exception e)
                {
                    if (tries >= Maxtries)
                    {
                        if (thrw)
                        {
                            System.Diagnostics.Debug.WriteLine("EXCEPTION ROOF");
                            System.Diagnostics.Debug.WriteLine(e);
                            throw;
                        }
                        else if (_totalRetries < Maxtries)
                        {
                            _totalRetries++;
                            tries = 0;
                        }
                        else
                        {
                            thrw = true;
                        }
                    }
                }
            }
        }
    }

    internal class LayerViolationException : Exception
    {
    }
}