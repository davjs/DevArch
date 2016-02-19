using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis.MSBuild;
using Project = EnvDTE.Project;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Integration
{
    public class AdvancedSolution
    {
        public readonly Solution RoslynSolution;
        public readonly Projects DteProjects;
        private readonly string _fullName;
        public readonly string Name;

        public string Directory;

        public AdvancedSolution(_DTE dte)
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
            Directory = Path.GetDirectoryName(_fullName) + "\\";
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