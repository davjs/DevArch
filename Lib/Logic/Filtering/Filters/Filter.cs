using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public abstract class Filter
    {
        private readonly FilterFunction _func;
        public bool ShouldBeApplied { get; }
        public string Name => GetType().Name;

        public void Apply(Node tree)
        {
            _func(tree);
        }

        protected delegate void FilterFunction(Node n); 
        protected Filter(bool shouldBeApplied, FilterFunction func)
        {
            _func = func;
            ShouldBeApplied = shouldBeApplied;
        }

        public Filter WithParameter(bool b)
        {
            return Activator.CreateInstance(GetType(), new[] { b }) as Filter;
        }
    }
}