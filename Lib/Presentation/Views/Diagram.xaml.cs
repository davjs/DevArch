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

        public Diagram(ArchViewModel model)
        {
            InitializeComponent();
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(new LayerView(layer));
            }
        }

        public void RenderModel(ArchViewModel model)
        {
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(new LayerView(layer));
            }
        }
    }
}