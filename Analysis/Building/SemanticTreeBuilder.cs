using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Analysis.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Analysis.Building
{
    static public class SemanticTreeBuilder
    {
        public static Tree BuildDependenciesFromReferences(Tree tree)
        {
            tree.UpdateChildren(tree.Childs.Select(x => BuildDependenciesFromReferences(x, tree)).ToList());
            return tree;
        }

        public static Node BuildDependenciesFromReferences(Node node,Tree root)
        {
            node.UpdateChildren(node.Childs.Select(x => BuildDependenciesFromReferences(x, root)).ToList());
            foreach (var reference in node.References)
            {
                var usedAt = reference.Locations.Where(x => !x.IsImplicit);
                var usedBy = usedAt.FindReferencingSymbolsAsync(default(CancellationToken)).Result;

                foreach (var nodeUsingThisNode in usedBy.Select(root.FindNodeWithSymbol))
                {
                    nodeUsingThisNode.Dependencies.Add(node);
                }
            }
            return node;
        }

        public static IEnumerable<Node> BuildTreeFromClasses(IEnumerable<ClassInfo> classes)
        {
            var all = new List<Node>();
            var classList = classes.ToList();
            foreach (var @class in classList)
            {
                FindParent(ref all, classList, @class.Symbol);
            }
            return all.Where(x => x.Parent == null);
        }

        private static Node FindParent(ref List<Node> all, List<ClassInfo> classList, ISymbol symbol)
        {
            var node = all.Find(x => Equals(x.Symbol, symbol));
            if (node != null)
                return node;
            var classInfo = classList.FirstOrDefault(x => Equals(x.Symbol, symbol));
            node = classInfo != null ? new Node(classInfo) : new Node(symbol);
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