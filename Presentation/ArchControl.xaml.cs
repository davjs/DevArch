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
using Analysis.SemanticTree;
using EnvDTE;
using Color = System.Drawing.Color;

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

        public LayerControl RenderNode(INodeViewModel node, int depth, System.Windows.Media.Color color)
        {
            var orientation = node.Horizontal ? Orientation.Horizontal : Orientation.Vertical;
            var layer = new LayerControl(node.Name, color, orientation);
            if (depth <= 0) return layer;
            foreach (var c in node.Childs)
            {
                layer.AddChild(RenderNode(c, depth--, color));
            }
            return layer;
        }

        public void RenderModel(ITreeViewModel model)
        {
            var c = System.Windows.Media.Color.FromRgb(0,43,54);
            foreach (var child in model.Childs)
            {
                MasterPanel.Children.Add(RenderNode(child, 3,c));
            }
        }
    }
}