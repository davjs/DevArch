using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering
{    public static class IndirectSiblingBuilder
    {
        public static HashSet<Node> IndirectSiblingDependencies(this Node n, ISet<Node> exclude = null)
        {
            if (exclude == null)
                exclude = new HashSet<Node> { n };
            else
                exclude.Add(n);
            var sibDeps = n.SiblingDependencies.Except(exclude).ToHashSet();
            foreach (var dependency in sibDeps.ToHashSet())
            {
                sibDeps.UnionWith(IndirectSiblingDependencies(dependency, exclude));
            }
            return sibDeps;
        }
    }
}