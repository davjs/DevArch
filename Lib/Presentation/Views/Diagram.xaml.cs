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

        private static LayerView RenderNode(LayerViewModel layerModel)
        {
            var childs = layerModel.Children.Select(RenderNode).ToList();
            var layerView = new LayerView(layerModel, childs,
                !layerModel.Anonymous);
            return layerView;
        }

        public void RenderModel(ArchViewModel model)
        {
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(RenderNode(layer));
            }
        }
    }
}