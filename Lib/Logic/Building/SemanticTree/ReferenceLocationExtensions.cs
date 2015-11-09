using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Logic.Building.SemanticTree
{
    /*
    internal static class SemanticModelExtensions
    {
        internal static partial class SpecializedCollections
        {

            private partial class Empty
            {
                internal class Array<T>
                {
                    public static readonly T[] Instance = new T[0];
                }
            }

            public static readonly byte[] EmptyBytes = EmptyArray<byte>();
            public static readonly object[] EmptyObjects = EmptyArray<object>();

            public static T[] EmptyArray<T>()
            {
                return Empty.Array<T>.Instance;
            }

            public static IEnumerator<T> EmptyEnumerator<T>()
            {
                return Empty.Enumerator<T>.Instance;
            }

            public static IEnumerable<T> EmptyEnumerable<T>()
            {
                return Empty.List<T>.Instance;
            }

            public static ICollection<T> EmptyCollection<T>()
            {
                return Empty.List<T>.Instance;
            }

            public static IList<T> EmptyList<T>()
            {
                return Empty.List<T>.Instance;
            }

            public static IReadOnlyList<T> EmptyReadOnlyList<T>()
            {
                return Empty.List<T>.Instance;
            }

            public static ISet<T> EmptySet<T>()
            {
                return Empty.Set<T>.Instance;
            }

            public static IDictionary<TKey, TValue> EmptyDictionary<TKey, TValue>()
            {
                return Empty.Dictionary<TKey, TValue>.Instance;
            }

            public static IEnumerable<T> SingletonEnumerable<T>(T value)
            {
                return new Singleton.Collection<T>(value);
            }

            public static ICollection<T> SingletonCollection<T>(T value)
            {
                return new Singleton.Collection<T>(value);
            }

            public static IEnumerator<T> SingletonEnumerator<T>(T value)
            {
                return new Singleton.Enumerator<T>(value);
            }

            public static IEnumerable<T> ReadOnlyEnumerable<T>(IEnumerable<T> values)
            {
                return new ReadOnly.Enumerable<IEnumerable<T>, T>(values);
            }

            public static ICollection<T> ReadOnlyCollection<T>(ICollection<T> collection)
            {
                return collection == null || collection.Count == 0
                    ? EmptyCollection<T>()
                    : new ReadOnly.Collection<ICollection<T>, T>(collection);
            }

            public static ISet<T> ReadOnlySet<T>(ISet<T> set)
            {
                return set == null || set.Count == 0
                    ? EmptySet<T>()
                    : new ReadOnly.Set<ISet<T>, T>(set);
            }

            public static ISet<T> ReadOnlySet<T>(IEnumerable<T> values)
            {
                var set = values as ISet<T>;
                if (set != null)
                {
                    return ReadOnlySet(set);
                }

                HashSet<T> result = null;
                foreach (var item in values)
                {
                    result = result ?? new HashSet<T>();
                    result.Add(item);
                }

                return ReadOnlySet(result);
            }
        }


        internal static class SymbolInfoExtensions
        {
            public static IEnumerable<ISymbol> GetAllSymbols(this SymbolInfo info)
            {
                return info.Symbol == null && info.CandidateSymbols.Length == 0
                    ? SpecializedCollections.EmptyEnumerable<ISymbol>()
                    : GetAllSymbolsWorker(info).Distinct();
            }

            private static IEnumerable<ISymbol> GetAllSymbolsWorker(this SymbolInfo info)
            {
                if (info.Symbol != null)
                {
                    yield return info.Symbol;
                }

                foreach (var symbol in info.CandidateSymbols)
                {
                    yield return symbol;
                }
            }

            public static ISymbol GetAnySymbol(this SymbolInfo info)
            {
                return info.GetAllSymbols().FirstOrDefault();
            }

            public static IEnumerable<ISymbol> GetBestOrAllSymbols(this SymbolInfo info)
            {
                if (info.Symbol != null)
                {
                    return SpecializedCollections.SingletonEnumerable(info.Symbol);
                }
                else if (info.CandidateSymbols.Length > 0)
                {
                    return info.CandidateSymbols;
                }

                return SpecializedCollections.EmptyEnumerable<ISymbol>();
            }
        }

        public static SemanticMap GetSemanticMap(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return SemanticMap.From(semanticModel, node, cancellationToken);
        }

        /// <summary>
        /// Gets semantic information, such as type, symbols, and diagnostics, about the parent of a token.
        /// </summary>
        /// <param name="semanticModel">The SemanticModel object to get semantic information
        /// from.</param>
        /// <param name="token">The token to get semantic information from. This must be part of the
        /// syntax tree associated with the binding.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, SyntaxToken token, CancellationToken cancellationToken)
        {
            return semanticModel.GetSymbolInfo(token.Parent, cancellationToken);
        }

        public static TSymbol GetEnclosingSymbol<TSymbol>(this SemanticModel semanticModel, int position, CancellationToken cancellationToken)
            where TSymbol : ISymbol
        {
            for (var symbol = semanticModel.GetEnclosingSymbol(position, cancellationToken);
                 symbol != null;
                 symbol = symbol.ContainingSymbol)
            {
                if (symbol is TSymbol)
                {
                    return (TSymbol)symbol;
                }
            }

            return default(TSymbol);
        }

        public static ISymbol GetEnclosingNamedTypeOrAssembly(this SemanticModel semanticModel, int position, CancellationToken cancellationToken)
        {
            return semanticModel.GetEnclosingSymbol<INamedTypeSymbol>(position, cancellationToken) ??
                (ISymbol)semanticModel.Compilation.Assembly;
        }

        public static INamedTypeSymbol GetEnclosingNamedType(this SemanticModel semanticModel, int position, CancellationToken cancellationToken)
        {
            return semanticModel.GetEnclosingSymbol<INamedTypeSymbol>(position, cancellationToken);
        }

        public static INamespaceSymbol GetEnclosingNamespace(this SemanticModel semanticModel, int position, CancellationToken cancellationToken)
        {
            return semanticModel.GetEnclosingSymbol<INamespaceSymbol>(position, cancellationToken);
        }

        public static ITypeSymbol GetType(
            this SemanticModel semanticModel,
            SyntaxNode expression,
            CancellationToken cancellationToken)
        {
            var typeInfo = semanticModel.GetTypeInfo(expression, cancellationToken);
            var symbolInfo = semanticModel.GetSymbolInfo(expression, cancellationToken);
            return typeInfo.Type ?? symbolInfo.GetAnySymbol().ConvertToType(semanticModel.Compilation);
        }

        public static IEnumerable<ISymbol> GetSymbols(
            this SemanticModel semanticModel,
            SyntaxToken token,
            Workspace workspace,
            bool bindLiteralsToUnderlyingType,
            CancellationToken cancellationToken)
        {
            var languageServices = workspace.Services.GetLanguageServices(token.Language);
            var syntaxFacts = languageServices.GetService<ISyntaxFactsService>();
            if (!syntaxFacts.IsBindableToken(token))
            {
                return SpecializedCollections.EmptyEnumerable<ISymbol>();
            }

            var semanticFacts = languageServices.GetService<ISemanticFactsService>();

            return GetSymbolsEnumerable(
                            semanticModel, semanticFacts, syntaxFacts,
                            token, bindLiteralsToUnderlyingType, cancellationToken)
                           .WhereNotNull()
                           .Select(MapSymbol);
        }

        private static ISymbol MapSymbol(ISymbol symbol)
        {
            return symbol.IsConstructor() && symbol.ContainingType.IsAnonymousType
                ? symbol.ContainingType
                : symbol;
        }
        
        public static IEnumerable<ISymbol> GetSymbolsEnumerable(
            SemanticModel semanticModel,
            SyntaxToken token,
            CancellationToken cancellationToken)
        {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);
            if (declaredSymbol != null)
            {
                yield return declaredSymbol;
                yield break;
            }

            var aliasInfo = semanticModel.GetAliasInfo(token.Parent, cancellationToken);
            if (aliasInfo != null)
            {
                yield return aliasInfo;
            }
            
            var symbol = semanticModel.GetSymbolInfo(token.Parent, cancellationToken).Symbol;
            var type = semanticModel.GetTypeInfo(token.Parent, cancellationToken).Type;

            if (type != null && symbol == null)
            {
                if (type.Kind == SymbolKind.NamedType)
                {
                    var namedType = (INamedTypeSymbol)type;
                    if (namedType.TypeKind == TypeKind.Delegate ||
                        namedType.AssociatedSymbol != null)
                    {
                        yield return type;
                    }
                }
            }
            if (symbol.IsThisParameter() && type != null)
            {
                yield return type;
            }
            else if (symbol.IsFunctionValue())
            {
                var method = symbol.ContainingSymbol as IMethodSymbol;

                if (method != null)
                {
                    if (method.AssociatedSymbol != null)
                    {
                        yield return method.AssociatedSymbol;
                    }
                    else
                    {
                        yield return method;
                    }
                }
                else
                {
                    yield return symbol;
                }
            }
            else
            {
                yield return symbol;
            }
        }
    }*/


    public static class ReferenceLocationExtensions
    {
        public static List<ISymbol> FindReferencingSymbolsAsync(
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
                var compilation = project.GetCompilationAsync(cancellationToken);
                foreach (var documentGroup in projectGroup)
                {
                    var document = documentGroup.Key;
                    AddSymbolsAsync(document, documentGroup, result, cancellationToken);
                }
                GC.KeepAlive(compilation);
            }

            return result;
        }

        private static void AddSymbolsAsync(Document document, IEnumerable<ReferenceLocation> references, List<ISymbol> result, CancellationToken cancellationToken)
        {
            var semanticModel = document.GetSemanticModelAsync(cancellationToken).Result;
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