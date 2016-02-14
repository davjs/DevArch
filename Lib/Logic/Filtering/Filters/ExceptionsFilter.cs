using System;
using System.Linq;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class ExceptionsFilter : ClassNodeFilter
    {
        public ExceptionsFilter(bool shouldBeApplied) : base(shouldBeApplied) { }

        protected override Func<ClassNode, bool> filter => x 
            => x.BaseClasses.Any(y => y.ToString() == "Exception");
    }
}