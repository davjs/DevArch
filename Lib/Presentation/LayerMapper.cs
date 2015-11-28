using System.Collections.Generic;
using System.Linq;
using Logic.Building.SemanticTree;
using Presentation.Coloring;
using Presentation.Coloring.ColoringAlgorithms;
using Presentation.ViewModels;
using static Logic.Building.SemanticTree.OrientationKind;

namespace Presentation
{
    public static class LayerMapper
    {
        static readonly IPalletteAlgorithm Pallette = new HueRangeDivisor();

        public static ArchViewModel TreeModelToArchViewModel(Tree model)
        {
            return new ArchViewModel
            {
                Layers = PaintAndMapNodes(model.Childs).ToList()
            };
        }

        private static IEnumerable<LayerViewModel> PaintAndMapNodes(IEnumerable<Node> nodes)
        {
            var layers = nodes.Select(NodeViewModelToLayerViewModel).ToList();
            return PaintLayers(layers).ToList();
        }

        public static IEnumerable<LayerViewModel> PaintLayers(List<LayerViewModel> layers,
            IColorData parentColorData = null,LayerViewModel parent = null,int depth = 0)
        {
            var toPaintDistinct = new List<LayerViewModel>();
            var toPaintSameColor = new List<LayerViewModel>();
            if (parent != null && parent.Anonymous)
            {
                toPaintSameColor = layers;
            }
            else
            {
                if (depth == 0  && layers.Sum(x => x.Descendants) < 5)
                {
                    toPaintDistinct = layers;
                }
                else
                {
                    //TODO: Dont do this if the levels of the tree is less than 3?
                    toPaintDistinct = layers.Where(l => l.Children.Any() && !l.Anonymous).ToList();
                    toPaintSameColor = layers.Where(l => !l.Children.Any() || l.Anonymous).ToList();
                }
            }
            var distinctColors = new Stack<IColorData>();
            if (parentColorData == null)
                parentColorData = Pallette.GetStartingColorData();
            if (toPaintDistinct.Count > 1)
                distinctColors = Pallette.GetDistinctColors(parentColorData,toPaintDistinct.Count);
            if(toPaintDistinct.Count == 1)
                distinctColors = new Stack<IColorData>(Pallette.GetDistinctColors(parentColorData,2).Take(1));

            foreach (var layer in toPaintSameColor)
            {
                var color = Pallette.GetSubColor(parentColorData);
                PaintLayer(layer, color);
            }

            foreach (var layer in toPaintDistinct)
            {
                var colorData = distinctColors.Pop();
                PaintLayer(layer, colorData);
            }

            return layers;
        }

        private static void PaintLayer(LayerViewModel layer, IColorData colorData,int depth = 0)
        {
            layer.Color = colorData.GetColor();
            layer.Children = PaintLayers(layer.Children.ToList(), colorData, layer,depth + 1);
        }

        public static LayerViewModel NodeViewModelToLayerViewModel(Node node)
        {
            var column = 0;
            var row = 0;

            var children = node.Childs.Select(NodeViewModelToLayerViewModel).ToList();
            foreach (var childLayer in children)
            {
                childLayer.Column = column;
                childLayer.Row = row;
                if (node.Orientation == Horizontal)
                    column++;
                else
                    row++;
            }
            return new LayerViewModel
            {
                Columns = column,
                Rows = row,
                Name = node.Name /* + (color.Bottom + color.Top) /2*/,
                Anonymous = !node.Name.Any(),
                Children = children,
                Descendants = children.Count + children.Sum(model => model.Descendants)
            };
        }
    }
}