using System;
using System.Linq;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering.Filters
{
    public abstract class NodeFilter : Filter
    {
        protected NodeFilter(bool shouldBeApplied, Func<Node, bool> filter) : base(shouldBeApplied,node => Apply(node,filter))
        {
        }

        private static void Apply(Node tree, Func<Node, bool> filter)
        {
            var toRemove = tree.Childs.Where(filter).ToList();
            toRemove.ForEach(tree.FilterChild);
            tree.Childs.ForEach(x => Apply(x,filter));
        }
    }
}