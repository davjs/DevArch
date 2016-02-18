using System;
using System.Linq;
using Logic.SemanticTree;
using MoreLinq;

namespace Logic.Filtering.Filters
{
    public abstract class BooleanClassPredicateFilter : Filter
    {
        protected BooleanClassPredicateFilter(bool shouldBeApplied, Func<ClassNode, bool> predicate) : 
            base(shouldBeApplied,node => ApplyPredicate(node,predicate))
        {
        }

        private static void ApplyPredicate(Node tree,Func<ClassNode,bool> predicate)
        {
            var classes = tree.Childs.OfType<ClassNode>();
            var toRemove = classes.Where(predicate).ToList();
            toRemove.ForEach(tree.RemoveChild);
            tree.Childs.ForEach(x => ApplyPredicate(x, predicate));
        }
    }

    public abstract class IntegralClassPredicateFilter : IntegralFilter
    {
        protected IntegralClassPredicateFilter(int parameter, Func<ClassNode,int, bool> predicate) : 
            base(parameter, (node, i) => ApplyPredicate(node,parameter,predicate))
        {
        }

        private static void ApplyPredicate(Node tree,int param, Func<ClassNode, int, bool> predicate)
        {
            var classes = tree.Childs.OfType<ClassNode>();
            var toRemove = classes.Where(x => predicate(x, param)).ToList();
            toRemove.ForEach(tree.RemoveChild);
            tree.Childs.ForEach(x => ApplyPredicate(x, param, predicate));
        }
    }
}