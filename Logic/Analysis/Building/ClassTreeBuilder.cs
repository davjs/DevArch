using System;
using System.Linq;
using Logic.Analysis.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Logic.Analysis.Building
{
    public static class ClassTreeBuilder
    {
        public static void AddClassesToTree(Solution solution, Tree tree, string documentName = null)
        {
            foreach (var project in tree.DescendantNodes().OfType<ProjectNode>())
            {
                var documents = project.Documents.ToList();
                if (!documents.Any()) continue;

                if (documentName != null)
                    documents = documents.Where(d => d.Name == documentName).ToList();

                var semanticModels = documents.Select(d => d.GetSemanticModelAsync().Result);
                var classes = SemanticModelWalker.GetClassesInModels(semanticModels, solution);
                if (!classes.Any())
                    continue;
                var classnodes = SemanticTreeBuilder.BuildTreeFromClasses(classes);
                project.AddChilds(classnodes);
            }
        }
    }
}