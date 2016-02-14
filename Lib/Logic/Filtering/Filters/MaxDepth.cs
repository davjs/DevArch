using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class MaxDepth : IntegralFilter
    {
        public MaxDepth(int i) : base(i)
        {
        }

        public override void Apply(Node tree)
        {
            ModelFilterer.RemoveNodesWithMoreDepthThan(tree, Parameter);
        }
    }
}