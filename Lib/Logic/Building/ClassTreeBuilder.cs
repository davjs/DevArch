using System;
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
            var allClassesBySymbol = new Dictionary<ISymbol, ClassNode>();

            var projects = tree.DescendantNodes().OfType<ProjectNode>().ToList();
            
            var semantics = new SemanticModelWalker.SemanticModels(projects.SelectMany(p => p.Documents.Select(d => d.GetSemanticModelAsync().Result)).ToList());

            foreach (var project in projects)
            {
                var documents = project.Documents.ToList();
                if (!documents.Any()) continue;

                if (documentName != null)
                    documents = documents.Where(d => d.Name == documentName).ToList();

                var semanticModels = documents.Select(d => d.GetSemanticModelAsync().Result).ToList();
                var classes = SemanticModelWalker.GetClassesInModels(semanticModels, solution, semantics);
                if (!classes.Any())
                    continue;

                foreach (var classNode in classes)
                {
                    allClassesBySymbol.Add(classNode.Symbol,classNode);
                }
                
                var classnodes = BuildTreeFromClasses(classes);
                project.AddChilds(classnodes);
            }

            foreach (var @class in allClassesBySymbol.Values)
            {
                foreach (var dependency in @class.SymbolDependencies)
                {
                    var contained =  allClassesBySymbol.ContainsKey(dependency);
                    if (contained)
                        @class.Dependencies.Add(allClassesBySymbol[dependency]);
                    else
                    {
                        var matchingSymbols = allClassesBySymbol.Keys.Where(x => SymbolsMatch(x,dependency)).ToList();
                        if(matchingSymbols.Count() == 1)
                            @class.Dependencies.Add(allClassesBySymbol[matchingSymbols.First()]);
                        if (matchingSymbols.Count() > 1)
                            throw new NotImplementedException();
                    }
                }
                
            }
        }

        private static bool SymbolsMatch(ISymbol symbol, ISymbol dependency)
        {
            return Equals(symbol, dependency) ||
                symbol.MetadataName == dependency.MetadataName
                && SymbolsMatch(symbol.ContainingSymbol,dependency.ContainingSymbol);
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