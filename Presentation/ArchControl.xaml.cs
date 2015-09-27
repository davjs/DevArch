//------------------------------------------------------------------------------
// <copyright file="ArchControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Analysis;
using EnvDTE;

namespace Presentation
{
    /// <summary>
    /// Interaction logic for ArchControl.
    /// </summary>
    public partial class ArchControl : UserControl
    {
        private readonly DTE _enviroment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchControl"/> class.
        /// </summary>
        /// <param name="enviroment"></param>
        public ArchControl(DTE enviroment)
        {
            _enviroment = enviroment;
            InitializeComponent();
        }

        public ArchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void GenerateDiagram(object sender, RoutedEventArgs e)
        {
            GenerateDiagram(_enviroment);
        }
        public void GenerateDiagram(DTE enviroment)
        {
            var model = Analyser.AnalyseEnviroment(enviroment);
            RenderModel(model);
        }

        /*public StackPanel RenderChilrenToPanel()
        {
            
        }*/

        /*public void RenderModel(ITreeViewModel model)
        {
            foreach (var child in model.Childs)
            {
                var layerContents = new StackPanel ();
                layerContents.Children.Add(new Button {Content = child.Name});
                var layerBorder = new Border { Style = Resources["LayerBorderStyle"] as Style};
                if (child.Childs.Any())
                {
                    var layerChildren = new StackPanel { Orientation = Orientation.Horizontal };
                    foreach (var c in child.Childs)
                    {
                        layerChildren.Children.Add(new Button {Content = c.Name});
                    }
                    layerContents.Children.Add(layerChildren);
                }
                layerBorder.Child = layerContents;
                MasterPanel.Children.Add(layerBorder);
            }
        }*/

        public LayerControl RenderNode(INodeViewModel node)
        {
            var layer = new LayerControl(node.Name);
            foreach (var c in node.Childs)
            {
                layer.AddChild(RenderNode(c));
            }
            return layer;
        }

        public void RenderModel(ITreeViewModel model)
        {
            foreach (var child in model.Childs)
            {
                MasterPanel.Children.Add(RenderNode(child));
            }
        }


    }
}