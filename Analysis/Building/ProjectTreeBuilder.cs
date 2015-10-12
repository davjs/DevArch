using System.Linq;
using System.Runtime.InteropServices;
using Analysis.SemanticTree;
using EnvDTE;
using EnvDTE80;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Analysis.Building
{
    public static class ProjectTreeBuilder
    {
        public static void AddProjectsToTree(Solution solution, ref Tree tree)
        {
            var projects = solution.Projects.ToList();
            var allreadyAddedProjects = tree.DescendantNodes().OfType<ProjectNode>().ToList();

            foreach (var project in projects)
            {
                var existingProject = allreadyAddedProjects.WithName(project.Name);
                if (existingProject != null)
                {
                    existingProject.Documents = project.Documents.ToList();
                }
                else
                {
                    existingProject = new ProjectNode(project);
                    tree.AddChild(existingProject);
                }
            }
        }

        public static
            Tree GetSolutionFoldersTree(DTE dte)
        {
            var tree = new Tree();
            var solution2 = GetSolution2(dte);
            var projects = solution2.Projects;
            if (projects == null) return tree;
            foreach (
                var folder in projects.Cast<Project>()
                .Where(x => x.Kind == ProjectKinds.vsProjectKindSolutionFolder))
            {
                var node = GetSolutionFolderNode(folder);
                tree.AddChild(node);
            }
            return tree;
        }

        private static Node GetSolutionFolderNode(Project folder)
        {
            var node = new Node(folder.Name);
            var subProjects = folder.ProjectItems.Cast<ProjectItem>().Select(GetProjectItemNode).ToList();
            node.AddChilds(subProjects);
            return node;
        }

        private static Node GetProjectItemNode(ProjectItem project)
        {
            if (project.SubProject == null
                || project.SubProject.Kind != ProjectKinds.vsProjectKindSolutionFolder) return new ProjectNode(project);
            return GetSolutionFolderNode(project.SubProject);
        }

        private static Solution2 GetSolution2(DTE dte)
        {
            EnvDTE.Solution sol = null;
            while (sol == null)
            {
                try
                {
                    sol = dte.Solution;
                }
                catch (COMException)
                {
                    // ignored
                }
            }
            // ReSharper disable once SuspiciousTypeConversion.Global
            return (sol as Solution2);
        }
    }
}