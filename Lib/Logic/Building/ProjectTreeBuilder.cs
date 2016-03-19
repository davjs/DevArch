using System;
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
        /*public static void AddDocumentsToProjects(Solution solution, ref SolutionNode tree)
        {
            var allreadyAddedProjects = tree.DescendantNodes().OfType<ProjectNode>().ToList();
            foreach (var project in allreadyAddedProjects)
            {
                project.Documents = solution.GetProject(ProjectId.CreateFromSerialized(project.ProjectId)).Documents.ToList();
            }
        }*/
        public static void AddDocumentsToProjects(Solution solution, ref SolutionNode tree)
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
                    throw new Exception($@"Project '{project.Name}' found by roslyn was not found by EnvDte or MSBuild");
            }
        }

        public static IEnumerable<Node> BuildProjectTree(Projects projects)
        {
            return projects.Cast<Project>()
                .Select(GetProjectItemNode);
        }

        private static Node GetSolutionFolderNode(Project folder)
        {
            var node = new Node(folder.Name);
            var childs = folder.ProjectItems.Cast<ProjectItem>().Where(x => x.SubProject != null)
                .Select(x => GetProjectItemNode(x.SubProject)).ToList();
            node.AddChilds(childs);
            return node;
        }

        private static Node GetProjectItemNode(Project project)
        {
            return project.Kind != ProjectKinds.vsProjectKindSolutionFolder
                ? ProjectNode.FromEnvDteProject(project) 
                : GetSolutionFolderNode(project);
        }
    }
}