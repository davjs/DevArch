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
            var groupedByLevel = classes.ToLookup(x => GetNameSpaceDepth(x.NameSpace), info => info);
            var topLevel = new List<Node>();
            var previousLevel = topLevel;
            foreach (var classesInLevel in groupedByLevel.OrderBy(x => x.Key))
            {
                var currentLevel = new List<Node>();
                foreach (var @class in classesInLevel)
                {
                    var nspace = currentLevel.Find(x => x.Name == @class.NameSpace.Name);
                    if (nspace == null)
                    {
                        nspace = new Node(@class.NameSpace);
                        currentLevel.Add(nspace);
                        var containedNamespace = @class.NameSpace.ContainingNamespace;
                        if (containedNamespace != null)
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
        }

        public static int GetNameSpaceDepth(INamespaceSymbol namespaceSymbol)
        {
            if (namespaceSymbol.IsGlobalNamespace)
                return 0;
            if (namespaceSymbol.ContainingNamespace != null)
                return 1 + GetNameSpaceDepth(namespaceSymbol.ContainingNamespace);
            return namespaceSymbol.Name.Count(x=> x == '.');
        }
    }
}