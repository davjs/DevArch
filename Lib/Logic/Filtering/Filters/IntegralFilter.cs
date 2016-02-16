using System;
using System.Linq.Expressions;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class IntegralFilter : Filter
    {
        private readonly IntegerFilterFunc _f;
        public delegate void IntegerFilterFunc(Node n, int i);
        protected int Parameter { get; }

        protected IntegralFilter(int parameter,IntegerFilterFunc f) :
            base(parameter > 0,node => f(node,parameter))
        {
            _f = f;
            Parameter = parameter;
        }

        public IntegralFilter WithParameter(int newParameter)
        {
            return new IntegralFilter(newParameter,_f);
        }
    }
}