using System.Linq;
using System.Windows.Controls;
using Presentation.ViewModels;

namespace Presentation.Views
{
    public partial class Diagram
    {
        public Diagram()
        {
            InitializeComponent();
        }

        public Diagram(ArchViewModel model)
        {
            InitializeComponent();
            RenderModel(model);
        }

        public void RenderModel(ArchViewModel model)
        {
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(ViewModelGenerator.CreateViewFromViewModel(layer).UiElement);
            }
        }
    }
}