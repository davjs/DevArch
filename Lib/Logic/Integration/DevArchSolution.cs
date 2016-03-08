﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Logic.Building;
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
            Solution = new DevArchSolution(environment);
        }
        public VisualStudio(DTE environment,Solution RoslynSolution)
        {
            _automationObject = (DTE2)environment;
            Solution = new DevArchSolution(environment,RoslynSolution);
        }
    }

    public class DevArchSolution
    {
        public readonly Solution RoslynSolution;
        public Projects DteProjects;
        public string _fullName;
        public string Name;
        public string Directory;
        public Node SolutionTree;
        /*public DevArchSolution(_DTE dte)
        {
            var build = MSBuildWorkspace.Create();
            var dteSolution = KeepTrying.ToGet(() => dte.Solution);
            _fullName = KeepTrying.ToGet(() => dteSolution.FullName);
            if (string.IsNullOrEmpty(_fullName))
                throw new Exception("Unable to find opened solution");
            var sol = build.OpenSolutionAsync(_fullName);
            RoslynSolution = KeepTrying.ToGet(() => sol.Result);

            DteProjects = KeepTrying.ToGet(() =>dteSolution.Projects);
            Name = Path.GetFileName(_fullName);
            if (_fullName == null)
                throw new NoSolutionOpenException();
        }*/

        public DevArchSolution(_DTE dte, Solution currentSolution = null)
        {
            SolutionTree = GetDteProjects(dte);
            if (currentSolution != null)
            {
                RoslynSolution = currentSolution;
            }
            else
            {
                var build = MSBuildWorkspace.Create();
                var sol = build.OpenSolutionAsync(_fullName);
                sol.Wait();
                RoslynSolution = KeepTrying.ToGet(() => sol.Result);
                if (!RoslynSolution.Projects.Any())
                    throw new NoCsharpProjectsFoundException();
            }
        }

        public DevArchSolution(string path)
        {
            _fullName = path;
            var SlnNode = new SolutionNode("-");
            SlnNode.AddChilds(GetProjectTree(_fullName));
            //
            var build = MSBuildWorkspace.Create();
            var sol = build.OpenSolutionAsync(_fullName);
            sol.Wait();
            RoslynSolution = KeepTrying.ToGet(() => sol.Result);
            if (!RoslynSolution.Projects.Any())
                throw new NoCsharpProjectsFoundException();
        }
        public static IEnumerable<Node> GetProjectTree(string path)
        {
            var x = SolutionFile.Parse(path);
            var isFolder = x.ProjectsInOrder.ToLookup(pI => pI.ProjectType == SolutionProjectType.SolutionFolder);
            var folders = isFolder[true].Select(f => new ProjectNode(f)).ToList();
            var projects = isFolder[false].Select(p => new ProjectNode(p)).ToList();
            var all = folders.Union(projects).ToList();
            foreach (var project in x.ProjectsInOrder)
            {
                var currNode = all.First(p => p.ProjectId == new Guid(project.ProjectGuid));
                if (project.ParentProjectGuid != null)
                {
                    var parentId = new Guid(project.ParentProjectGuid);
                    var parent = folders.First(f => f.ProjectId == parentId);
                    parent.AddChild(currNode);
                }
                else
                {
                    yield return currNode;
                }
            }
        }

        private SolutionNode GetDteProjects(_DTE dte)
        {
            var dteSolution = dte.Solution;
            _fullName = KeepTrying.ToGet(() => dteSolution.FullName);
            if (string.IsNullOrEmpty(_fullName))
                throw new Exception("Unable to find opened solution");
            DteProjects = KeepTrying.ToGet(() => dteSolution.Projects);
            Name = Path.GetFileName(_fullName);
            Directory = Path.GetDirectoryName(_fullName) + "\\";
            var sln = new SolutionNode("-");
            sln.AddChilds(ProjectTreeBuilder.AddSolutionFoldersToTree(DteProjects));
            return sln;
        }

        public IList<ArchProject> ArchProjects
        {
            get
            {
                var projects = new List<ArchProject>();
                foreach (Project project in DteProjects)
                {
                    try
                    {
                        var fullName = project.FullName;
                        if (fullName.EndsWith(".archproj"))
                            projects.Add(new ArchProject(project));
                    }
                    catch (Exception)
                    {
                        // ignored, happens when trying to access the full name of an unloaded project
                    }
                }
                return projects;
            }
        }

        private class NoSolutionOpenException : Exception
        {
        }
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