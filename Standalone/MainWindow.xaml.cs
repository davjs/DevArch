using System.Collections.Generic;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic;
using Presentation;
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
            //TODO: Generalize?
            var modelGen = new DiagramFromModelDefinitionGenerator(GetDte());
            var tree = modelGen.GenerateDiagram(ModelDefinition.RootDefault);
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree);
            ArchControl.RenderModel(viewModel);
        }


        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}
