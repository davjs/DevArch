using System;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Logic.Filtering.Filters
{
    public class MinMethods : IntegralClassPredicateFilter
    {
        public MinMethods(int i) : base(i, ShouldBeRemoved)
        {
        }

        public static bool ShouldBeRemoved(ClassNode node,int x)
        {
            return node.NrOfMethods < x;
        }
    }
}