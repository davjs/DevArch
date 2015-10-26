using System.Linq;
using System.Threading;
using Logic.Building.SemanticTree;

namespace Logic.Building
{
    static public class DependencyBuilder
    {
        public static Tree BuildDependenciesFromReferences(Tree tree)
        {
            tree.SetChildren(tree.Childs.Select(x => BuildDependenciesFromReferences(x, tree)).ToList());
            return tree;
        }

        public static Node BuildDependenciesFromReferences(Node node,Tree root)
        {
            node.SetChildren(node.Childs.Select(x => BuildDependenciesFromReferences(x, root)).ToList());
            if( node is ClassNode)
            { 
                foreach (var reference in (node as ClassNode).References)
                {
                    var usedAt = reference.Locations.Where(x => !x.IsImplicit);
                    var usedBy = usedAt.FindReferencingSymbolsAsync(default(CancellationToken)).Result;

                    foreach (var nodeUsingThisNode in usedBy.Select(root.FindNodeWithSymbol))
                    {
                        nodeUsingThisNode?.Dependencies.Add(node);
                    }
                }
            }
            return node;
        }
    }
}