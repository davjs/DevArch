using System.Linq;
using Analysis.SemanticTree;
using EnvDTE;
using EnvDTE80;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Analysis.Building
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

        public static
            Tree GetSolutionFoldersTree(DTE dte)
        {
            var tree = new Tree();
            var projects = (dte.Solution as Solution2)?.Projects;
            if (projects == null) return tree;
            foreach (Project project in projects)
            {
                if (project?.Kind != ProjectKinds.vsProjectKindSolutionFolder) continue;
                var node = new Node(project.Name);
                var subProjects = project.ProjectItems.Cast<ProjectItem>().Select(p => new ProjectNode(p));
                node.AddChilds(subProjects);
                tree.AddChild(node);
            }
            return tree;
        }
    }
}