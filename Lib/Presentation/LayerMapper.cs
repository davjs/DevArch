using System.Collections.Generic;
using System.Linq;
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
                Layers = model.Childs.Select(NodeViewModelToLayerViewModel)
            };
        }

        public static LayerViewModel NodeViewModelToLayerViewModel(Node node)
        {
            var children = new List<LayerViewModel>();

            var column = 0;
            var row = 0;

            foreach (var childLayer in node.Childs.Select(NodeViewModelToLayerViewModel))
            {
                childLayer.Column = column;
                childLayer.Row = row;
                children.Add(childLayer);
                if (node.Horizontal)
                    column++;
                else
                    row++;
            }
            return new LayerViewModel {Columns = column,Rows = row, Name = node.Name, Anonymous = !node.Name.Any(), Children = children}; 
        }
        
    }
}
