using System.Windows.Controls;
using System.Windows.Markup;

namespace Lib
{
    /// <summary>
    /// Interaction logic for Diagram.xaml
    /// </summary>
    [ContentProperty("MainContent")]
    public partial class ArchView : UserControl
    {
        public ArchView()
        {
            InitializeComponent();
        }
    }
}
