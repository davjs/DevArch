using System;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Logic.Filtering.Filters
{
    public class MinMethods : IntegralClassPredicateFilter
    {
        public MinMethods(int i) : base(i, MethodsLessThan)
        {
        }

        public static bool MethodsLessThan(ClassNode node,int x)
        {
            return node.NrOfMethods < x;
        }
    }
}