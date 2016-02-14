using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public abstract class Filter
    {
        public bool ShouldBeApplied { get; }
        public string Name => GetType().Name;
        public abstract void Apply(Node tree);

        protected Filter(bool shouldBeApplied)
        {
            ShouldBeApplied = shouldBeApplied;
        }

        public Filter WithParameter(bool b)
        {
            return Activator.CreateInstance(GetType(), new[] { b }) as Filter;
        }
    }
}