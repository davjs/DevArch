using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;
using Project = EnvDTE.Project;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Building
{
    public static class ProjectTreeBuilder
    {
        public static void AddDocumentsToProjects(Solution solution, ref SolutionNode tree)
        {
            var allreadyAddedProjects = tree.DescendantNodes().OfType<ProjectNode>().ToList();
            foreach (var project in allreadyAddedProjects)
            {
                project.Documents = solution.GetProject(ProjectId.CreateFromSerialized(project.ProjectId)).Documents.ToList();
            }
        }
        public static void AddProjectsToTree(Solution solution, ref SolutionNode tree)
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
                    var newProject = new ProjectNode(project);
                    tree.AddChild(newProject);
                }
            }
        }

        public static
            IEnumerable<Node> AddSolutionFoldersToTree(Projects projects)
        {
            return projects.Cast<Project>()
                .Where(x => x.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                .Select(GetSolutionFolderNode);
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
    }
}