using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Building.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Logic.Building
{
    public static class SemanticModelWalker
    {
        public static IEnumerable<ClassNode> GetClassesInModel(SemanticModel model, Solution solution,
            SemanticModels semantics)
        {
            var syntaxRoot = model.SyntaxTree.GetRootAsync().Result;
            var classes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var classAnalysis = classes.Select(c => CreateClassNode(model, c, solution, semantics));
            return classAnalysis;
        }


        /*public static IEnumerable<ClassNode> GetClassesInModels(IEnumerable<SemanticModel> models, Solution solution)
        {

            var symbolsPerClass = new Dictionary<ClassDeclarationSyntax, List<ISymbol>>();


            foreach (var model in models)
            {
                var allClasses = model.SyntaxTree.GetRootAsync().Result.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var @class in allClasses)
                {
                    var declaredSymbol = model.GetDeclaredSymbol(@class);
                    symbolsPerClass[@class] = new List<ISymbol>();
                    symbolsPerClass[@class].Add(declaredSymbol);

                    foreach (var node in @class.DescendantNodes())
                    {
                        var symbol = model.GetSymbolInfo(node).Symbol;
                        var type = model.GetTypeInfo(node).Type;
                        var aliasInfo = model.GetAliasInfo(node);
                        if(symbol != null)
                    }

                }
            }
            //var classAnalysis = allClasses.Select(c => CreateClassNode(model, c, solution));
            return null;
        }*/


        private static ClassNode CreateClassNode(SemanticModel model, ClassDeclarationSyntax c, Solution solution, SemanticModels semantics)
        {
            var declaredSymbol = model.GetDeclaredSymbol(c);

            var subnodes = c.DescendantNodes();
            GetDependenciesFromNodes(subnodes, model);
            //var symbols = subnodes.Select(semantics.GetSymbol).ToList();
            var symbolInfos = subnodes.Select(node => model.GetSymbolInfo(node)).ToList();
            var subSymbols = symbolInfos.Select(x => x.Symbol).ToList();
            
            var dependencies =
                subSymbols.Where(symbol => symbol is INamedTypeSymbol)
                    .Cast<INamedTypeSymbol>()
                    .Select(x => x.OriginalDefinition);
            //var references = SymbolFinder.FindReferencesAsync(declaredSymbol,solution ).Result;
            IEnumerable<TypeSyntax> basetypes = new List<TypeSyntax>();
            if (c.BaseList != null && c.BaseList.Types.Any())
                basetypes = c.BaseList.Types.Select(x => x.Type);
            return new ClassNode(declaredSymbol, null, basetypes) {SymbolDependencies = dependencies};
        }

        private static void GetDependenciesFromNodes(IEnumerable<SyntaxNode> subnodes, SemanticModel model)
        {
            foreach (var node in subnodes)
            {
                if (node is IdentifierNameSyntax)
                {
                    var declared = model.GetDeclaredSymbol((node as IdentifierNameSyntax));
                }
            }
        }

        /*

    
            var symbol = semanticModel.GetSymbolInfo(token.Parent, cancellationToken).Symbol;
            var type = semanticModel.GetTypeInfo(token.Parent, cancellationToken).Type;
            var aliasInfo = semanticModel.GetAliasInfo(token.Parent, cancellationToken);
            var declaredSymbol = semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);
        */

        public static IReadOnlyList<ClassNode> GetClassesInModels(IList<SemanticModel> semanticModels,
            Solution solution, SemanticModels semantics)
        {
            return semanticModels.SelectMany(
                model => GetClassesInModel(model, solution, semantics)).Distinct().ToList();
        }


        public class SemanticModels
        {
            private readonly IList<SemanticModel> _models;

            public SemanticModels(IList<SemanticModel> models)
            {
                _models = models;
            }

            public ISymbol GetSymbol(SyntaxNode syntax)
            {
                foreach (var model in _models)
                {
                    //SymbolInfo? symbolInfo = null;
                    ISymbol symbolInfo = null;
                    try
                    {
                        symbolInfo = model.GetAliasInfo(syntax);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    if (symbolInfo != null)
                        return symbolInfo;
                    //if (symbolInfo?.CandidateSymbols != null)
                    //    return symbolInfo?.CandidateSymbols.FirstOrDefault();
                }

                return null;
            }
        }
    }
}