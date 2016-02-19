using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class MinReferences : IntegralClassPredicateFilter
    {
        public MinReferences(int i) : base(i, ShouldBeRemoved)
        {
        }

        private static bool ShouldBeRemoved(ClassNode arg1, int arg2)
        {
            return arg1.References.Count < arg2;
        }
    }
}