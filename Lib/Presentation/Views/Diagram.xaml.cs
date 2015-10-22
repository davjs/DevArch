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

        private static LayerView RenderNode(LayerViewModel layerModel, int depth, AdvancedColor color)
        {
            depth -= 1;
            var oldColor = color.Clone();
            var childs = new List<LayerView>();
            if (!layerModel.Anonymous)
            {
                color.L *= 1.1;
                color.S *= 1.2;
            }
            foreach (var child in layerModel.Children)
            {
                childs.Add(RenderNode(child, depth, color.Clone()));
            }

            var layerView = new LayerView(layerModel, oldColor, childs, layerModel.Column, layerModel.Row,
                !layerModel.Anonymous,layerModel.Columns,layerModel.Rows);
            return layerView;
        }

        public void RenderModel(ArchViewModel model)
        {
            var colors = new Stack<AdvancedColor>(Colors.GetNColors(model.Layers.Count()));
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(RenderNode(layer, 10, colors.Pop()));
            }
        }
    }
}