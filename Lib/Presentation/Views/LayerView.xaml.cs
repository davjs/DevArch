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
    public interface IDiagramControl
    {
        int Column { get; set; }
        int Row { get; set; }

        UIElement UiElement { get; }
    }

    /// <summary>
    /// Interaction logic for layerControl.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class LayerView : IDiagramControl
    {

        public LayerView()
        {
            InitializeComponent();
        }

        public LayerView(LayerViewModel layerModel)
        {
            InitializeComponent();
            LayerName = layerModel.Name;
            NameBlock.Text = layerModel.Name;
            Row = layerModel.Row;
            Column = layerModel.Column;
            var childMargin = CalculateChildMargin(layerModel);
            var visible = !layerModel.Anonymous;
            var childs = layerModel.Children.Select(ViewModelGenerator.CreateViewFromViewModel).ToList();
            foreach (var child in childs)
            {
                if(child is LayerView)
                    (child as LayerView).Border.Margin = childMargin;
                ChildHolder.Children.Add(child.UiElement);
                Grid.SetColumn(child.UiElement, child.Column);
                Grid.SetRow(child.UiElement, child.Row);
            }

            for (var i = 0; i < layerModel.Rows; i++)
                ChildHolder.RowDefinitions.Add(new RowDefinition());
            
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
            int height;
            int width;
            //var maxHeight = 10;
            //var minHeight = 1;
            //var height = Math.Min(Math.Max((layerModel.Descendants* layerModel.Descendants) /120, minHeight), maxHeight);

            height = 5;
            width = 5;

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
            //Border.BorderThickness = new Thickness(0);
            //Border.Margin = new Thickness(0);
            //Border.Background = null;
        }

        public string LayerName { get; set; }
        public int Row { get; set; }
        public UIElement UiElement => this;
        public int Column { get; set; }
    }
}
