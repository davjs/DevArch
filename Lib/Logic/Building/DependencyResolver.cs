using System;
using System.Collections.Generic;
using System.Linq;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Logic.Building
{
    public static class DependencyResolver
    {
        public static void ResolveDependencies(IEnumerable<ClassNode> allClasses)
        {
            var allClassesBySymbol = allClasses.ToDictionary(x => x.Symbol, x => x);

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
                        var matchingSymbols = allClassesBySymbol.Keys.Where(x => SymbolsMatch(x, dependency)).ToList();
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
    }
}