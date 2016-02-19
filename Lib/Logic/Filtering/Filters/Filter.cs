using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class Filter
    {
        private readonly FilterFunction _func;
        public bool ShouldBeApplied { get; }
        public string Name { get; }

        public void Apply(Node tree)
        {
            _func(tree);
        }

        protected delegate void FilterFunction(Node n); 
        protected Filter(bool shouldBeApplied, FilterFunction func,string name = null)
        {
            _func = func;
            ShouldBeApplied = shouldBeApplied;
            Name = name ?? GetType().Name;
        }

        public Filter WithParameter(bool b)
        {
            return new Filter(b, _func,GetType().Name);
        }
    }
}