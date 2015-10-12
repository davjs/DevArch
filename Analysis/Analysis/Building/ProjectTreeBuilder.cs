using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Logic.Analysis.SemanticTree;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Analysis.Building
{
    public static class ProjectTreeBuilder
    {
        public static void AddProjectsToTree(Solution solution,ref Tree tree)
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

        public static void AddProjectToTree(Solution solution, ref Tree tree, string name)
        {
            var project = solution.Projects.First(p => p.Name == name);
            tree.AddChild(new ProjectNode(project));
        }

        public static
            Tree AddSolutionFoldersToTree(Projects projects)
        {
            var tree = new Tree();
            if (projects == null) return tree;
            foreach (Project project in projects)
            {
                if (project?.Kind != ProjectKinds.vsProjectKindSolutionFolder) continue;
                var node = new Node(project.Name);
                var items = project.ProjectItems.Cast<ProjectItem>();

                var projectNodes = new List<ProjectNode>();
                foreach (var item in items)
                {
                    Node itemNode = null;
                    if (item?.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    {
                        itemNode = new Node(item.Name);
                    }
                }
                var subProjects = items.Select(p => new ProjectNode(p));
                node.AddChilds(subProjects);
                tree.AddChild(node);
            }
            return tree;
        }
    }
}