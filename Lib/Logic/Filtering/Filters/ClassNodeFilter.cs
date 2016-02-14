using System;
using System.Linq;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering.Filters
{
    public abstract class ClassNodeFilter : Filter
    {
        public ClassNodeFilter(bool shouldBeApplied) : base(shouldBeApplied)
        {
        }

        protected abstract Func<ClassNode, bool> filter { get; }

        public override void Apply(Node tree)
        {
            var classes = tree.Childs.OfType<ClassNode>();
            var toRemove = classes.Where(filter).ToList();
            toRemove.ForEach(tree.RemoveChild);
            tree.Childs.ForEach(Apply);
        }
    }
}