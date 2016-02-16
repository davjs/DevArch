using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class MinReferences : IntegralFilter
    {
        public MinReferences(int i) : base(i, Apply)
        {
        }

        private static void Apply(Node tree, int i1)
        {
            throw new NotImplementedException();
        }
    }
}