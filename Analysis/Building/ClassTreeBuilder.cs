using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Analysis
{
    public static class ClassTreeBuilder
    {
        public static void AddClassesToTree(Solution solution, Tree tree)
        {
            foreach (var project in tree.DescendantNodes().OfType<ProjectNode>())
            {
                var documents = project.Documents.ToList();
                if (documents.Any())
                {
                    var semanticModels = documents.Select(d => d.GetSemanticModelAsync().Result);
                    var classes = SemanticModelWalker.GetClassesInModels(semanticModels, solution);
                    if (!classes.Any())
                        throw new Exception("No classes found");
                    var classnodes = SemanticTreeBuilder.BuildTreeFromClasses(classes);
                    project.AddChilds(classnodes);
                }
            }
        }
    }
}