using System.Collections.Generic;
using System.Linq;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Logic.Building
{
    public static class SemanticModelWalker
    {
        private static IEnumerable<ClassNode> GetClassesInModel(SemanticModel model)
        {
            var syntaxRoot = model.SyntaxTree.GetRootAsync().Result;
            var classes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var classAnalysis = classes.Select(c => CreateClassNode(model, c));
            return classAnalysis;
        }

        private static ClassNode CreateClassNode(SemanticModel model, ClassDeclarationSyntax c)
        {
            var declaredSymbol = model.GetDeclaredSymbol(c);

            var subnodes = c.DescendantNodes();
            var symbols = subnodes.Select(node => model.GetSymbolInfo(node).Symbol).ToList();

            var dependencies = symbols.OfType<INamedTypeSymbol>();
            var nrOfMethods = subnodes.OfType<MethodDeclarationSyntax>().Count();

            IEnumerable<TypeSyntax> basetypes = new List<TypeSyntax>();
            if (c.BaseList != null && c.BaseList.Types.Any())
                basetypes = c.BaseList.Types.Select(x => x.Type);
            return new ClassNode(declaredSymbol, basetypes, nrOfMethods) {SymbolDependencies = dependencies};
        }

        public static IReadOnlyList<ClassNode> GetClassesInModels(IEnumerable<SemanticModel> semanticModels)
        {
            return semanticModels.SelectMany(GetClassesInModel).Distinct().ToList();
        }
    }
}