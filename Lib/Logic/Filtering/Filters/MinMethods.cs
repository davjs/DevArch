using System;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;

namespace Logic.Filtering.Filters
{
    public class MinMethods : IntegralFilter
    {
        public MinMethods(int i) : base(i,Apply)
        {
        }

        private static void Apply(Node tree, int i1)
        {
            throw new NotImplementedException();
        }
    }
}