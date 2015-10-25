using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Logic.Building.SemanticTree
{
    public static class ReferenceLocationExtensions
    {
        public static async Task<List<ISymbol>> FindReferencingSymbolsAsync(
            this IEnumerable<ReferenceLocation> referenceLocations,
            CancellationToken cancellationToken)
        {
            var documentGroups = referenceLocations.GroupBy(loc => loc.Document);
            var projectGroups = documentGroups.GroupBy(g => g.Key.Project);
            var result = new List<ISymbol>();

            foreach (var projectGroup in projectGroups)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var project = projectGroup.Key;
                var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
                foreach (var documentGroup in projectGroup)
                {
                    var document = documentGroup.Key;
                    await AddSymbolsAsync(document, documentGroup, result, cancellationToken).ConfigureAwait(false);
                }

                GC.KeepAlive(compilation);
            }

            return result;
        }

        private static async Task AddSymbolsAsync(
            Document document,
            IEnumerable<ReferenceLocation> references,
            List<ISymbol> result,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            foreach (var reference in references)
            {
                var containingSymbol = GetEnclosingClassOrNamespace(semanticModel, reference);
                if (containingSymbol == null) continue;
                if (!result.Contains(containingSymbol))
                {
                    result.Add(containingSymbol);
                }
            }
        }

        private static ISymbol GetEnclosingClassOrNamespace(
            SemanticModel semanticModel,
            ReferenceLocation reference)
        {
            var enclosingSymbol = semanticModel.GetEnclosingSymbol(reference.Location.SourceSpan.Start);

            var current = enclosingSymbol;

            while (current.Kind != SymbolKind.NamedType && current.Kind != SymbolKind.Namespace)
            {
                current = current.ContainingSymbol;
                if (current == null)
                    return null;
            }

            return current;
        }
    }
}