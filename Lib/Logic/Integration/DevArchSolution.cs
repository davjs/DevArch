using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Logic.Building;
using Logic.Common;
using Logic.SemanticTree;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis.MSBuild;
using Project = EnvDTE.Project;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Integration
{
    public class VisualStudio
    {
        public readonly DevArchSolution Solution;
        private readonly DTE2 _automationObject;

        public OutputWindowPane DevArchOutputWindow()
        {
            if (_automationObject == null)
                return null;

            var outputWindow = _automationObject.ToolWindows.OutputWindow;
            outputWindow.Parent.Activate();
            OutputWindowPane devArchOutput;
            try
            {
                devArchOutput = outputWindow.OutputWindowPanes.Item("DevArch");
            }
            catch (Exception)
            {
                devArchOutput = null;
            }
            if (devArchOutput == null)
                devArchOutput = outputWindow.OutputWindowPanes.Add("DevArch");
            devArchOutput.Activate();
            return devArchOutput;
        }

        public VisualStudio(DTE environment)
        {
            _automationObject = (DTE2) environment;
            Solution = DevArchSolution.FromCurrentInstance(environment);
        }
        public VisualStudio(DTE environment,Solution roslynSolution)
        {
            _automationObject = (DTE2)environment;
            Solution = DevArchSolution.FromCurrentInstanceWithSolution(environment,roslynSolution);
        }
    }

    public class DevArchSolution
    {
        public readonly Solution RoslynSolution;
        public readonly string Directory;
        public readonly SolutionNode SolutionTree;

        private DevArchSolution(IEnumerable<Node> tree,Solution roslynSolution)
        {
            if (!roslynSolution.Projects.Any())
                throw new NoCsharpProjectsFoundException();

            RoslynSolution = roslynSolution;
            var fullName = roslynSolution.FilePath;
            var name = Path.GetFileName(fullName);
            Directory = Path.GetDirectoryName(fullName) + "\\";
            
            SolutionTree = new SolutionNode(name);
            SolutionTree.AddChilds(tree);
            var allProjects = SolutionTree.DescendantNodes().OfType<ProjectNode>();

            // Remove unloaded projects from tree
            var unloadedProjects = allProjects.Where(x => !x.ProjectProperties.isLoaded).ToList();
            foreach (var project in unloadedProjects)
                project.Parent.RemoveChild(project);

            // Remove arch projects from tree
            var archProjects = allProjects.Where(x => x.ProjectProperties.Path.EndsWith(".archproj")).ToList();
            foreach (var archProject in archProjects)
                archProject.Parent.RemoveChild(archProject);
            
            ArchProjects = archProjects.Select(x => new ArchProject(x.ProjectProperties)).ToList();
        }

        public static DevArchSolution FromCurrentInstance(_DTE dte)
        {
            var dteSolution = dte.Solution;
            var _fullName = KeepTrying.ToGet(() => dteSolution.FullName);
            return new DevArchSolution(GetProjectTreeFromDte(dte),GetRoslynSolutionFromPath(_fullName));
        }

        public static DevArchSolution FromCurrentInstanceWithSolution(_DTE dte,Solution currentSolution) =>
            new DevArchSolution(GetProjectTreeFromDte(dte), currentSolution);

        public static DevArchSolution FromPath(string path)
        {
            return new DevArchSolution(GetProjectTreeFromPath(path), GetRoslynSolutionFromPath(path));
        }

        private static Solution GetRoslynSolutionFromPath(string path)
        {
            var build = MSBuildWorkspace.Create();
            var sol = build.OpenSolutionAsync(path);
            sol.Wait();
            return KeepTrying.ToGet(() => sol.Result);
        }
        
        // Slower but doesn't require a visual studio instance
        private static IEnumerable<Node> GetProjectTreeFromPath(string path)
        {
            var solFile = SolutionFile.Parse(path);
            
            var projects = solFile.ProjectsInOrder.ToList();
            var all = projects.SelectList(ProjectNode.FromMsBuildProject);
            var nodes = new List<Node>();
            foreach (var project in solFile.ProjectsInOrder)
            {
                var currNode = all.FirstOrDefault(p => p.ProjectProperties.Id == new Guid(project.ProjectGuid));
                if (currNode == null)
                    continue;
                if (project.ParentProjectGuid != null)
                {
                    var parentId = new Guid(project.ParentProjectGuid);
                    var parent = all.First(f => f.ProjectProperties.Id == parentId);
                    parent.AddChild(currNode);
                }
                else
                {
                    nodes.Add(currNode);
                }
            }
            return nodes;
        }
        
        private static IEnumerable<Node> GetProjectTreeFromDte(_DTE dte)
        {
            var dteSolution = dte.Solution;
            var fullName = KeepTrying.ToGet(() => dteSolution.FullName);
            if (string.IsNullOrEmpty(fullName))
                throw new Exception("Unable to find opened solution");
            var dteProjects = KeepTrying.ToGet(() => dteSolution.Projects);
            return ProjectTreeBuilder.BuildProjectTree(dteProjects);
        }

        public readonly List<ArchProject> ArchProjects;
    }

    public static class ProjectExtensions
    {
        private static readonly Func<ProjectItem, bool> IsFolder = x => x.ProjectItems.Count != 0;

        public static IEnumerable<ProjectItem> GetAllProjectItems(this Project project)
        {
            var areFolder = project.ProjectItems.Cast<ProjectItem>().ToLookup(IsFolder);
            var items = areFolder[false].ToList();
            var folders = areFolder[true].ToList();
            return items.Concat(folders.SelectMany(GetAllProjectItems));
        }

        private static IEnumerable<ProjectItem> GetAllProjectItems(ProjectItem folder)
        {
            var areFolder = folder.ProjectItems.Cast<ProjectItem>().ToLookup(IsFolder);
            var items = areFolder[false];
            var folders = areFolder[true];
            return items.Concat(folders.SelectMany(GetAllProjectItems));
        }
    }


    static class KeepTrying
    {
        static int _totalRetries = 0;
        const int Maxtries = 3;

        public static T ToGet<T>(Func<T> function) where T : class
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

                    if (_totalRetries > Maxtries)
                    {
                        throw e;
                    }

                    _totalRetries++;
                    tries = 0;
                }
            }
            return result;
        }
    }
}