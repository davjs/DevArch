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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for layerControl.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class LayerControl : UserControl
    {
        public Orientation Orientation { get; set; }

        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
           "Children",
           typeof(UIElementCollection),
           typeof(LayerControl),
           new PropertyMetadata());

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set {
                SetValue(ChildrenProperty, value);
            }
        }

        public void AddChild(LayerControl layer)
        {
            var count = ChildPanel.Children.Count;
            ChildPanel.Children.Add(layer);
            if(Orientation == Orientation.Horizontal)
            { 
                Grid.SetColumn(layer, count);
                ChildPanel.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            }
            else
            {
                Grid.SetRow(layer, count);
                ChildPanel.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
            }
        }

        public LayerControl(string name, Color color, Orientation orientation = Orientation.Vertical)
        {
            LayerName = name;
            InitializeComponent();
            Children = ChildPanel.Children;
            DataContext = this;
            Orientation = orientation;
            Border.Background = new SolidColorBrush(color);
            if (name.Any()) return;
            NameBlock.Visibility = Visibility.Collapsed;
            Border.BorderThickness = new Thickness(0);
            Border.Margin = new Thickness(0);
        }

        public string LayerName { get; set; }
    }
}
