using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class RemoveTests : NodeFilter
    {
        public RemoveTests(bool shouldBeApplied) : base(shouldBeApplied, filter) {}

        private static Func<Node, bool> filter { get; } = x =>
            x.Name.EndsWith("test", StringComparison.InvariantCultureIgnoreCase)
            || x.Name.EndsWith("tests", StringComparison.InvariantCultureIgnoreCase)
            || x.Name.EndsWith("testing", StringComparison.InvariantCultureIgnoreCase);
    }
}