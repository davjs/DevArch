using System.Collections.Generic;
using System.Linq;
using Logic.Building.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Logic.Building
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
                var classnodes = BuildTreeFromClasses(classes);
                project.AddChilds(classnodes);
            }
        }

        private static IEnumerable<Node> BuildTreeFromClasses(IEnumerable<ClassNode> classes)
        {
            var all = new List<Node>();
            var classList = classes.ToList();
            foreach (var @class in classList)
            {
                FindParent(ref all, classList, @class.Symbol);
            }
            return all.Where(x => x.Parent == null);
        }

        private static Node FindParent(ref List<Node> all, List<ClassNode> classList, ISymbol symbol)
        {
            var node = all.Find(x => Equals(x.Symbol, symbol));
            if (node != null)
                return node;
            var classInfo = classList.FirstOrDefault(x => Equals(x.Symbol, symbol));
            node = classInfo ?? new Node(symbol);
            all.Add(node);
            if (symbol.ContainingSymbol is INamespaceSymbol 
                && !((INamespaceSymbol) symbol.ContainingSymbol).IsGlobalNamespace 
                || symbol.ContainingSymbol is INamedTypeSymbol)
            {
                var parent = FindParent(ref all, classList, symbol.ContainingSymbol);
                parent.AddChild(node);
                return node;
            }

            return node;
        }
    }
}