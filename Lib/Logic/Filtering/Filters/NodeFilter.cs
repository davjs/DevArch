using System;
using System.Linq;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering.Filters
{
    public abstract class NodeFilter : Filter
    {
        public NodeFilter(bool shouldBeApplied) : base(shouldBeApplied)
        {
        }

        protected abstract Func<Node, bool> filter { get; }

        public override void Apply(Node tree)
        {
            var toRemove = tree.Childs.Where(filter).ToList();
            toRemove.ForEach(tree.RemoveChild);
            tree.Childs.ForEach(Apply);
        }
    }
}