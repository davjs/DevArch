using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Presentation.ViewModels;

namespace Presentation.Views
{
    /// <summary>
    /// Interaction logic for ArrowView.xaml
    /// </summary>
    public partial class ArrowView : IDiagramControl
    {
        public ArrowView(ArrowViewModel arrowViewModel)
        {
            DataContext = this;
            //TODO: Extract base constroctur
            Row = arrowViewModel.Row;
            Column = arrowViewModel.Column;
            Angle = arrowViewModel.Angle;
            InitializeComponent();
        }

        public int Angle { get; set; }

        public int Column { get; set; }
        public int Row { get; set; }
        public UIElement UiElement => this;
    }
}
