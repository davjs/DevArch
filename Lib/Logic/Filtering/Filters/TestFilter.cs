using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class TestFilter : NodeFilter
    {
        public TestFilter(bool shouldBeApplied) : base(shouldBeApplied, filter) {}

        private static Func<Node, bool> filter { get; } = x =>
            x.Name.EndsWith("test", StringComparison.InvariantCultureIgnoreCase)
            || x.Name.EndsWith("tests", StringComparison.InvariantCultureIgnoreCase);
    }
}