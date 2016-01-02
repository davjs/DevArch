using System.Linq;
using EnvDTE;
using EnvDTE80;
using Logic.SemanticTree;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Logic.Building
{
    public static class ProjectTreeBuilder
    {
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
                    existingProject = new ProjectNode(project);
                    tree.AddChild(existingProject);
                }
            }
        }

        public static void AddProjectToTree(Solution solution, ref SolutionNode tree, string name)
        {
            var project = solution.Projects.First(p => p.Name == name);
            tree.AddChild(new ProjectNode(project));
        }

        public static
            void AddSolutionFoldersToTree(Projects projects, ref SolutionNode tree)
        {
            if (projects == null) return;
            foreach (
                var folder in projects.Cast<Project>()
                .Where(x => x.Kind == ProjectKinds.vsProjectKindSolutionFolder))
            {
                var node = GetSolutionFolderNode(folder);
                tree.AddChild(node);
            }
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