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
            var palette = new Palette();
            var colors =  palette.RequestSubColorsFor(model.Childs.Count);
            return new ArchViewModel
            {
                Layers = model.Childs.Select(c => NodeViewModelToLayerViewModel(c,palette, colors.Pop())).ToList()
            };
        }

        public static LayerViewModel NodeViewModelToLayerViewModel(Node node, Palette palette, AdvancedColor color, int depth = 0)
        {
            var children = new List<LayerViewModel>();
            var column = 0;
            var row = 0;
            Stack<AdvancedColor> colors;
            if(depth >= 1)
                colors = palette.RequestSubColorsFor(color,200);
            else
            {
                colors = palette.RequestSubColorsFor(color, node.Childs.Count);
            }
            foreach (var childLayer in node.Childs.Select(node1 => NodeViewModelToLayerViewModel(node1, palette, colors.Pop(),depth +1)))
            {
                    
                childLayer.Column = column;
                childLayer.Row = row;
                children.Add(childLayer);
                if (node.Horizontal)
                    column++;
                else
                    row++;
            }
            return new LayerViewModel {Color = color,Columns = column,Rows = row, Name = node.Name, Anonymous = !node.Name.Any(), Children = children}; 
        }
        
    }
}
