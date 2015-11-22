using System;
using System.Collections.Generic;
using System.Linq;
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

        public LayerView(LayerViewModel layerModel)
        {
            InitializeComponent();
            LayerName = layerModel.Name;
            NameBlock.Text = layerModel.Name;
            _column = layerModel.Column;
            _row = layerModel.Row;
            var childMargin = CalculateChildMargin(layerModel);
            bool visible = !layerModel.Anonymous;
            var childs = layerModel.Children.Select(x => new LayerView(x)).ToList();
            foreach (var child in childs)
            {
                child.Border.Margin = childMargin;
                ChildHolder.Children.Add(child);
                Grid.SetColumn(child, child._column);
                Grid.SetRow(child, child._row);
            }

            for (var i = 0; i < layerModel.Rows; i++)
                ChildHolder.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});

            for (var i = 0; i < layerModel.Columns; i++)
                ChildHolder.ColumnDefinitions.Add(new ColumnDefinition());

            DataContext = this;

            var borderColor = CalculateBorderColor(layerModel.Color);
            Border.Background = new SolidColorBrush(layerModel.Color);
            Border.BorderBrush = new SolidColorBrush(borderColor);
            if (!childs.Any())
            {
                DockPanel.VerticalAlignment = VerticalAlignment.Center;
                Border.Background = new LinearGradientBrush(layerModel.Color, borderColor, 30);
            }
            if (!visible) Hide();
        }

        private static Thickness CalculateChildMargin(LayerViewModel layerModel)
        {
            var height = Math.Min(Math.Max((layerModel.Descendants* layerModel.Descendants) /120, 4),20);
            var width = 5;
            return new Thickness(width, height, width,height);
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
