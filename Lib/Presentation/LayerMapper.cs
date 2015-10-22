using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Logic.Analysis.SemanticTree;
using Presentation.ViewModels;

namespace Presentation
{
    public static class LayerMapper
    {
        public static ArchViewModel TreeModelToArchViewModel(Tree model)
        {
            return new ArchViewModel
            {
                Layers = PaintAndMapNodes(model.Childs).Item1
            };
        }

        private static Tuple<IEnumerable<LayerViewModel>,ColorRange> PaintAndMapNodes(IReadOnlyCollection<Node> nodes,ColorRange color = null)
        {
            List<ColorRange> subcolors;
            if (color != null)
            {
                subcolors = color.Divide(nodes.Count + 1).ToList();
                color = subcolors.First();
                subcolors.Remove(color);
            }
            else
            {
                color =new ColorRange();
                subcolors = color.Divide(nodes.Count).ToList();
            }
            var nodesAndColors = subcolors.Zip(nodes, (colorRange, subNode) => new { color = colorRange, node = subNode });
            return new Tuple<IEnumerable<LayerViewModel>, ColorRange>(nodesAndColors.Select(x => NodeViewModelToLayerViewModel(x.node,x.color)), color);
        }

        public static LayerViewModel NodeViewModelToLayerViewModel(Node node, ColorRange color)
        {
            var children = new List<LayerViewModel>();
            var column = 0;
            var row = 0;
            IEnumerable<LayerViewModel> childLayers;
            if (node.Horizontal)
            {
                var newColors = color.Divide(2);
                childLayers = node.Childs.Select(n => NodeViewModelToLayerViewModel(n, newColors.Last()));
                color = newColors.Last();
            }
            else
            {
                var paintedNodes = PaintAndMapNodes(node.Childs, color);
                childLayers = paintedNodes.Item1;
                color = paintedNodes.Item2;
            }
            foreach (var childLayer in childLayers)
            {       
                childLayer.Column = column;
                childLayer.Row = row;
                children.Add(childLayer);
                if (node.Horizontal)
                    column++;
                else
                    row++;
            }
            return new LayerViewModel {Color = color.GetColor(), Columns = column,Rows = row, Name = node.Name/* + (color.Bottom + color.Top) /2*/, Anonymous = !node.Name.Any(), Children = children}; 
        }
    }
}
