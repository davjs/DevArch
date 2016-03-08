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
        public static void AddClassesToTree(Node tree, string documentName = null)
        {
            var allClassesBySymbol = new Dictionary<ISymbol, ClassNode>();
            var projects = tree.DescendantNodes().OfType<ProjectNode>().ToList();
            
            foreach (var project in projects)
            {
                var documents = project.Documents.ToList();
                if (!documents.Any()) continue;

                if (documentName != null) { 
                    documents = documents.Where(d => d.Name == documentName).ToList();
                    if(!documents.Any())
                        throw new Exception("Unable to find document: " + documentName);
                }
                var semanticModels = documents.SelectList(d => d.GetSemanticModelAsync().Result);
                var classes = SemanticModelWalker.GetClassesInModels(semanticModels);
                if (!classes.Any())
                    continue;

                foreach (var classNode in classes)
                {
                    allClassesBySymbol.Add(classNode.Symbol,classNode);
                }
                
                var classnodes = BuildTreeFromClasses(classes);
                project.AddChilds(classnodes);
            }
            
            foreach (var dependor in allClassesBySymbol.Values)
            {
                foreach (var dependency in dependor.SymbolDependencies)
                {
                    if (allClassesBySymbol.ContainsKey(dependency))
                    {
                        CreateDependency(allClassesBySymbol[dependency], dependor);
                    }
                    else
                    {
                        var matchingSymbols = allClassesBySymbol.Keys.Where(x => SymbolsMatch(x,dependency)).ToList();
                        if (matchingSymbols.Count == 1)
                        {
                            CreateDependency(allClassesBySymbol[matchingSymbols.First()], dependor);
                        }
                        if (matchingSymbols.Count > 1)
                            throw new NotImplementedException();
                    }
                }
            }
        }

        private static void CreateDependency(ClassNode nodeDependantOn, Node dependor)
        {
            nodeDependantOn.References.Add(dependor);
            dependor.Dependencies.Add(nodeDependantOn);
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