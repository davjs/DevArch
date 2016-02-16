using Logic.SemanticTree;

namespace Logic.Filtering.Filters
{
    public class MaxDepth : IntegralFilter
    {
        public MaxDepth(int i) : base(i,Apply)
        {
        }

        private static void Apply(Node tree, int parameter)
        {
            ModelFilterer.RemoveNodesWithMoreDepthThan(tree, parameter);
        }
    }
}