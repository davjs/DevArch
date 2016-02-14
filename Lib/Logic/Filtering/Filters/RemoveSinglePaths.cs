using System;
using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class RemoveSinglePaths : Filter
    {
        public RemoveSinglePaths(bool parameter) : base(parameter)
        {
            
        }

        public override void Apply(Node tree)
        {
            throw new NotImplementedException();
        }
    }
}