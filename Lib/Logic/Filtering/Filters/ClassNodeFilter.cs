using System;
using System.Linq;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering.Filters
{
    public abstract class ClassNodeFilter : Filter
    {
        protected ClassNodeFilter(bool shouldBeApplied, Func<ClassNode, bool> filter) : base(shouldBeApplied,node => Apply(node,filter))
        {
            Filter = filter;
        }

        private Func<ClassNode, bool> Filter { get; }

        private static void Apply(Node tree,Func<ClassNode,bool> filter)
        {
            var classes = tree.Childs.OfType<ClassNode>();
            var toRemove = classes.Where(filter).ToList();
            toRemove.ForEach(tree.RemoveChild);
            tree.Childs.ForEach(x => Apply(x, filter));
        }
    }
}