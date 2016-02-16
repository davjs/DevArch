using System;
using System.Linq;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class ExceptionsFilter : ClassNodeFilter
    {
        public ExceptionsFilter(bool shouldBeApplied) : base(shouldBeApplied,FilterFunc) { }

        private static Func<ClassNode, bool> FilterFunc => x 
            => x.BaseClasses.Any(y => y.ToString() == "Exception");
    }
}