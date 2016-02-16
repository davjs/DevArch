using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class IntegralFilter : Filter
    {
        private Action<Node, int> F { get; }
        protected int Parameter { get; }

        protected IntegralFilter(int parameter,Action<Node,int> f) :
            base(parameter > 0,node => f(node,parameter))
        {
            F = f;
            Parameter = parameter;
        }

        public IntegralFilter WithParameter(int newParameter)
        {
            return new IntegralFilter(newParameter,F);
        }
    }
}