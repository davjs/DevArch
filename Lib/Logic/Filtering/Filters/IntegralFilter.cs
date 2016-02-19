using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class IntegralFilter : Filter
    {
        private Action<Node, int> F { get; }
        public int Parameter { get; }

        protected IntegralFilter(int parameter,Action<Node,int> f, string name = null) :
            base(parameter > 0,node => f(node,parameter),name)
        {
            F = f;
            Parameter = parameter;
        }

        public IntegralFilter WithParameter(int newParameter)
        {
            return new IntegralFilter(newParameter,F,GetType().Name);
        }
    }
}