using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Presentation.Coloring;
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
            var childMargin = GetChildMargin();
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

            var color = layerModel.Color;
            var borderColor = CalculateBorderColor(color);
            var textColor = CalculateTextColor(color);
            Border.Background = new SolidColorBrush(color);
            Border.BorderBrush = new SolidColorBrush(borderColor);
            NameBlock.Foreground = new SolidColorBrush(textColor);
            if (!childs.Any())
            {
                DockPanel.VerticalAlignment = VerticalAlignment.Center;
                Border.Background = new LinearGradientBrush(color, borderColor, 30);
            }
            if (layerModel.Anonymous) HideName();
            if (layerModel.Invisible) Hide();
        }

        private Color CalculateTextColor(AdvancedColor color)
        {
            var copy = color.Copy();
            if(color.L < 0.4)
                copy.L = copy.L + 0.2;
            else
                copy.L = copy.L - 0.2;
            copy.S = 0.6 - copy.L / 1.6;
            return copy;
        }

        private static Thickness GetChildMargin()
        {
            return new Thickness(5, 5, 5,5);
        }

        //TODO: Move to model?
        private static Color CalculateBorderColor(AdvancedColor backgroundColor)
        {
            var copy = backgroundColor.Copy();
            copy.S *= 0.9d;
            copy.L = copy.L*1.03 + 0.02;
            return copy;
        }

        private void HideName()
        {
            NameBlock.Visibility = Visibility.Collapsed;
        }


        private void Hide()
        {
            Border.BorderThickness = new Thickness(0);
            Border.Margin = new Thickness(0);
            Border.Background = null;
        }

        public string LayerName { get; set; }
        public int Row { get; set; }
        public UIElement UiElement => this;
        public int Column { get; set; }
    }
}
