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
            ChildPanel.Children.Add(layer);
            Grid.SetColumn(layer, ChildPanel.Children.Count -1);
            ChildPanel.ColumnDefinitions.Add(new ColumnDefinition());
        }

        public LayerControl(string name)
        {
            LayerName = name;
            InitializeComponent();
            Children = ChildPanel.Children;
            DataContext = this;
        }

        public string LayerName { get; set; }
    }
}
