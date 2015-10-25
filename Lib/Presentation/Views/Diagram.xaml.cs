using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Presentation.ViewModels;

namespace Presentation.Views
{

    public partial class Diagram : UserControl
    {
        public Diagram()
        {
            InitializeComponent();
        }

        private static LayerView RenderNode(LayerViewModel layerModel, int depth)
        {
            depth -= 1;
            var childs = new List<LayerView>();
            foreach (var child in layerModel.Children)
            {
                childs.Add(RenderNode(child, depth));
            }

            var layerView = new LayerView(layerModel, childs, layerModel.Column, layerModel.Row,
                !layerModel.Anonymous,layerModel.Columns,layerModel.Rows);
            return layerView;
        }

        public void RenderModel(ArchViewModel model)
        {
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(RenderNode(layer, 10));
            }
        }
    }
}