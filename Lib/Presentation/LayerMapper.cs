using System.Collections.Generic;
using System.Linq;
using Logic.SemanticTree;
using MoreLinq;
using Presentation.Coloring;
using Presentation.Coloring.ColoringAlgorithms;
using Presentation.ViewModels;
using static Logic.SemanticTree.OrientationKind;

namespace Presentation
{
    public static class LayerMapper
    {
        private static IPalletteAlgorithm _pallette = new HueRangeDivisor(1);

        public static ArchViewModel TreeModelToArchViewModel(Node model,bool dependencyDown, bool hideAnonymousNodes)
        {
            _pallette = new HueRangeDivisor(model.Height());
            return new ArchViewModel
            {
                Layers = PaintAndMapNodes(model.Childs,dependencyDown, hideAnonymousNodes)
            };
        }

        private static IEnumerable<DiagramSymbolViewModel> PaintAndMapNodes(IEnumerable<Node> nodes, bool dependencyDown, bool hideAnonymousNodes)
        {
            var layers = nodes.Select(x => NodeViewModelToLayerViewModel(x,hideAnonymousNodes));
            layers = PaintLayers(layers.ToList());

            var layerAndArrows = AddArrows(layers, Vertical, dependencyDown).ToList();
            layerAndArrows.OfType<LayerViewModel>().ForEach(model => SetupColumnAndRows(model, model.Children));

            return layerAndArrows;
        }

        private static IEnumerable<DiagramSymbolViewModel> AddArrows(IEnumerable<DiagramSymbolViewModel> layers,OrientationKind parentOrientation ,bool dependencyDown)
        {
            var viewModels = layers.ToList();
            if (!viewModels.OfType<LayerViewModel>().Any(x => x.Children.OfType<LayerViewModel>().Any(y => y.Children.Any())))
                return viewModels;

            var arrowdirection = dependencyDown ?
                ArrowViewModel.Direction.Down : ArrowViewModel.Direction.Up;

            foreach (var layer in viewModels.Cast<LayerViewModel>())
                layer.Children = AddArrows(layer.Children,layer.Orientation,dependencyDown);

            if (viewModels.Count <= 1 || parentOrientation == Horizontal)
                return viewModels;
            for (var i = 1; i < viewModels.Count; i += 2)
            {
                viewModels.Insert(i,new ArrowViewModel(arrowdirection));
            }
            return viewModels;
        }

        public static IEnumerable<LayerViewModel> PaintLayers(List<LayerViewModel> layers,
            IColorData parentColorData = null,LayerViewModel parent = null,int depth = 0)
        {
            var toPaintDistinct = new List<LayerViewModel>();
            var toPaintSameColor = new List<LayerViewModel>();
            if (parentColorData == null)
                parentColorData = _pallette.GetStartingColorData();
            IColorData sameColor;
            if (parent != null && parent.Invisible)
            {
                //Parent is invisible, paint all layers with the parent color
                toPaintSameColor = layers;
                sameColor = parentColorData;
            }
            else
            {
                sameColor = _pallette.GetSubColor(parentColorData);
                if (depth == 0  && layers.Sum(x => x.Descendants) < 5)
                {
                    toPaintDistinct = layers;
                }
                else
                {
                    //TODO: Maybe not require children if the tree is shorter than 3?
                    toPaintDistinct = layers.Where(l => l.Children.Any() && !l.Invisible).ToList();
                    toPaintSameColor = layers.Where(l => !l.Children.Any() || l.Invisible).ToList();
                }
            }
            var distinctColors = new Stack<IColorData>();
            if (toPaintDistinct.Count > 1)
                distinctColors = _pallette.GetDistinctColors(parentColorData,toPaintDistinct.Count);
            if(toPaintDistinct.Count == 1)
                distinctColors = new Stack<IColorData>(_pallette.GetDistinctColors(parentColorData,2).Take(1));

            foreach (var layer in toPaintSameColor)
            {
                PaintLayer(layer, sameColor);
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
            PaintLayers(layer.Children.OfType<LayerViewModel>().ToList(), colorData, layer,depth + 1);
        }

        public static LayerViewModel NodeViewModelToLayerViewModel(Node node, bool hideAnonymousNodes)
        {
            var children = node.Childs.Select(c => NodeViewModelToLayerViewModel(c, hideAnonymousNodes)).ToList();
            var anonymous = !node.Name.Any();
            return new LayerViewModel
            {
                Name = node.Name,
                Anonymous = anonymous,
                Invisible = hideAnonymousNodes && anonymous,
                Children = children,
                Orientation = node.Orientation,
                Descendants = children.Count + children.Sum(model => model.Descendants)
            };
        }

        private static void SetupColumnAndRows(LayerViewModel node, IEnumerable<DiagramSymbolViewModel> children)
        {
            children.OfType<LayerViewModel>().ForEach(model => SetupColumnAndRows(model, model.Children));

            var column = 0;
            var row = 0;
            foreach (var childLayer in children)
            {
                childLayer.Column = column;
                childLayer.Row = row;
                if (node.Orientation == Horizontal)
                    column++;
                else
                    row++;
            }
            node.Rows = row;
            node.Columns = column;
        }
    }
}