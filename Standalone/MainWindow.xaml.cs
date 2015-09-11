using System.Collections.Generic;
using System.Windows;
using Analysis;

namespace Standalone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var tree = new NodeViewModel("");
            var DAL = new NodeViewModel(Name = "Data Access");
            var LOGIC = new NodeViewModel(Name = "Logic");
            var PRES = new NodeViewModel(Name = "Presentation");
            var childs = new List<NodeViewModel>
            {
                DAL,LOGIC,PRES
            };
            tree.Childs = childs;
            ArchControl.RenderModel(tree);
        }
    }
}
