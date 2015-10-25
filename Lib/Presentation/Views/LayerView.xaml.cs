using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Presentation.ViewModels;
using Colors = Presentation.Coloring.Colors;

namespace Presentation.Views
{
    /// <summary>
    /// Interaction logic for layerControl.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class LayerView : UserControl
    {
        private readonly int _column;
        private readonly int _row;

        public LayerView()
        {
            InitializeComponent();
        }

        public LayerView(LayerViewModel layerModel, IEnumerable<LayerView> childs, int column, int row, bool visible, int columns, int rows)
        {
            InitializeComponent();
            LayerName = layerModel.Name;
            NameBlock.Text = layerModel.Name;
            _column = column;
            _row = row;

            foreach (var child in childs)
            {
                ChildHolder.Children.Add(child);
                Grid.SetColumn(child, child._column);
                Grid.SetRow(child, child._row);
            }

            for (var i = 0; i < rows; i++)
                ChildHolder.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});

            for (var i = 0; i < columns; i++)
                ChildHolder.ColumnDefinitions.Add(new ColumnDefinition());

            DataContext = this;


            Border.Background = new SolidColorBrush(layerModel.Color);
            Border.BorderBrush = new SolidColorBrush(CalculateBorderColor(layerModel.Color));
            if (!visible) Hide();
        }

        //TODO: Move to model?
        private static Color CalculateBorderColor(Color backgroundColor)
        {
            var hsl = Colors.Rgbhsl.RGB_to_HSL(backgroundColor);
            hsl.S *= 0.9;
            hsl.L *= 1.1;
            var borderColor = Colors.Rgbhsl.HSL_to_RGB(hsl);
            return borderColor;
        }

        private void Hide()
        {
            NameBlock.Visibility = Visibility.Collapsed;
            Border.BorderThickness = new Thickness(0);
            Border.Margin = new Thickness(0);
            Border.Background = null;
        }

        public string LayerName { get; set; }
    }
}
