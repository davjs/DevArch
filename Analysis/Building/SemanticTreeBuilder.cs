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

        /*public static IEnumerable<Node> BuildTreeFromClasses(IEnumerable<ClassInfo> classes)
        {
            var groupedByDepth = classes.ToLookup(x => GetDepth(x.Symbol), info => info);
            var topLevel = new List<Node>();
            var previousLevel = topLevel;
            foreach (var classesInLevel in groupedByDepth.OrderBy(x => x.Key))
            {
                var currentLevel = new List<Node>();
                foreach (var @class in classesInLevel)
                {
                    var nspace = currentLevel.Find(x => Equals(x.Symbol, @class.Symbol.ContainingSymbol))
                        ;//?? previousLevel.Find(x => Equals(x.Symbol, @class.Symbol.ContainingSymbol));
                    if (nspace == null)
                    {
                        nspace = new Node(@class.Symbol.ContainingSymbol);
                        currentLevel.Add(nspace);
                        var containedNamespace = nspace.Symbol.ContainingSymbol;
                        if (containedNamespace is INamespaceOrTypeSymbol)
                        {
                            var linkedNameSpace = previousLevel.Find(x => Equals(x.Symbol, containedNamespace));
                            if (linkedNameSpace == null)
                            {
                                linkedNameSpace = new Node(containedNamespace);
                                previousLevel.Add(linkedNameSpace);
                            }
                            linkedNameSpace.AddChild(nspace);
                        }
                        else
                        {
                            topLevel.Add(nspace);
                        }
                    }
                    nspace.AddChild(new Node(@class));
                }
                previousLevel = currentLevel;
            }
            return topLevel;
        }*/

        public static IEnumerable<Node> BuildTreeFromClasses(IEnumerable<ClassInfo> classes)
        {
            var all = new List<Node>();
            var classList = classes.ToList();
            for (var i = 0; i < classList.Count; i++)
            {
                var @class = classList[i];
                FindParent(ref all, ref classList, @class.Symbol);
            }
            return all.Where(x => x.Parent == null);
        }

        private static Node FindParent(ref List<Node> all, ref List<ClassInfo> classList, ISymbol symbol)
        {
            var node = all.Find(x => Equals(x.Symbol, symbol));
            if (node != null)
                return node;
            var classInfo = classList.FirstOrDefault(x => Equals(x.Symbol, symbol));
            if (classInfo != null)
            {
                node = new Node(classInfo);
            }
            else
            {
                node = new Node(symbol);
            }
            all.Add(node);
            if (symbol.ContainingSymbol is INamespaceOrTypeSymbol)
            {
                var parent = FindParent(ref all, ref classList, symbol.ContainingSymbol);
                parent.AddChild(node);
                return node;
            }

            return node;
        }

        public static int GetDepth(ISymbol symbol)
        {
            if (symbol.ContainingSymbol is INamespaceOrTypeSymbol)
                return 1 + GetDepth(symbol.ContainingSymbol);
            return symbol.Name.Count(x=> x == '.');
        }
    }
}