using System;
using System.Linq;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering.Filters
{
    public abstract class NodeFilter : Filter
    {
        public NodeFilter(bool shouldBeApplied, Func<Node, bool> filter) : base(shouldBeApplied,node => Apply(node,filter))
        {
            Filter = filter;
        }

        protected Func<Node, bool> Filter { get; }

        public static void Apply(Node tree, Func<Node, bool> filter)
        {
            var toRemove = tree.Childs.Where(filter).ToList();
            toRemove.ForEach(tree.RemoveChild);
            tree.Childs.ForEach(x => Apply(x,filter));
        }
    }
}