using System.Collections.Generic;
using System.Runtime.InteropServices;
using Analysis;
using Analysis.SemanticTree;
using EnvDTE;
using Window = System.Windows.Window;

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
            var DAL = new NodeViewModel("Data Access");
            var LOGIC = new NodeViewModel("Logic") { Dependencies = new List<INodeViewModel> { DAL } }; ;
            var PRES = new NodeViewModel("Presentation") {Dependencies = new List<INodeViewModel> {LOGIC} };
            var childs = new List<NodeViewModel>
            {
                DAL,LOGIC,PRES
            };
            tree.Childs = childs;
            ArchControl.GenerateDiagram(GetDte());
        }


        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}
