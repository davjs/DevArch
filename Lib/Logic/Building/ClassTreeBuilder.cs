using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Common;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Logic.Building
{
    public static class ClassTreeBuilder
    {
        public static Node AddClassesInDocument(Document doc)
        {
            var semanticModels = doc.GetSemanticModelAsync().Result;
            var classes = SemanticModelWalker.GetClassesInModel(semanticModels).ToList();
            DependencyResolver.ResolveDependencies(classes);
            var docNode = new Node(doc.Name);
            docNode.AddChilds(BuildTreeFromClasses(classes));
            return docNode;
        }

        public static void AddClassesInProjectsToTree(SolutionNode tree)
        {
            var projects = tree.DescendantNodes().OfType<ProjectNode>().ToList();
            var allClasses = new List<ClassNode>();
            
            foreach (var project in projects)
            {
                var documents = project.Documents.ToList();
                if (!documents.Any()) continue;
                
                var semanticModels = documents.SelectList(d => d.GetSemanticModelAsync().Result);
                var classes = SemanticModelWalker.GetClassesInModels(semanticModels);
                if (!classes.Any())
                    continue;
                allClasses.AddRange(classes);
                var classnodes = BuildTreeFromClasses(classes);
                project.AddChilds(classnodes);
            }

            DependencyResolver.ResolveDependencies(allClasses);
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

        private static Node FindParent(ref List<Node> all, IList<ClassNode> classList, ISymbol symbol)
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