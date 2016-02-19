using System;
using System.Linq;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class RemoveExceptions : BooleanClassPredicateFilter
    {
        public RemoveExceptions(bool shouldBeApplied) : base(shouldBeApplied,FilterFunc) { }

        private static Func<ClassNode, bool> FilterFunc => x 
            => x.BaseClasses.Any(y => y.ToString() == "Exception");
    }
}