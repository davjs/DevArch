using System.Runtime.InteropServices;
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
            Lib.DevArch.RenderCompleteDiagramToView(GetDte(), ref ArchView);
        }


        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}
