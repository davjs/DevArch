using System.Collections.Generic;
using System.Linq;
using Logic.Building.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Logic.Building
{
    public static class SemanticModelWalker
    {
        public static IEnumerable<ClassNode> GetClassesInModel(SemanticModel model, Solution solution)
        {
            var syntaxRoot = model.SyntaxTree.GetRootAsync().Result;
            var classes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var classAnalysis = classes.Select(c => GetClassInfo(model, c, solution));
            return classAnalysis;
        }

        private static ClassNode GetClassInfo(SemanticModel model, ClassDeclarationSyntax c, Solution solution)
        {
            var declaredSymbol = model.GetDeclaredSymbol(c);
            var references = SymbolFinder.FindReferencesAsync(declaredSymbol,solution ).Result;
            IEnumerable<TypeSyntax> basetypes = new List<TypeSyntax>();
            if (c.BaseList != null && c.BaseList.Types.Any())
                basetypes = c.BaseList.Types.Select(x => x.Type);
            return new ClassNode(declaredSymbol, references, basetypes);
        }

        public static IReadOnlyList<ClassNode> GetClassesInModels(IEnumerable<SemanticModel> semanticModels, Solution solution)
        {
            return semanticModels.SelectMany(model => GetClassesInModel(model, solution)).ToList();
        }
    }
}