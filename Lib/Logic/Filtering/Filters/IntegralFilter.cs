using System;
using System.Linq.Expressions;

namespace Logic.Filtering.Filters
{
    public abstract class IntegralFilter : Filter
    {
        protected int Parameter { get; }

        protected IntegralFilter(int parameter) : base(parameter > 0)
        {
            Parameter = parameter;
        }

        public IntegralFilter WithParameter(int newParameter)
        {
            return Activator.CreateInstance(GetType(), new[] { newParameter }) as IntegralFilter;
        }
    }
}