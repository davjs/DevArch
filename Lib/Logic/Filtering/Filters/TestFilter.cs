using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class TestFilter : NodeFilter
    {
        public TestFilter(bool shouldBeApplied) : base(shouldBeApplied){}

        protected override Func<Node, bool> filter => x =>
            x.Name.EndsWith("test", StringComparison.InvariantCultureIgnoreCase)
            || x.Name.EndsWith("tests", StringComparison.InvariantCultureIgnoreCase);
    }
}