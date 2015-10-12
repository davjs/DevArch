//------------------------------------------------------------------------------
// <copyright file="ArchView.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Analysis;
using EnvDTE;
using Presentation.ViewModels;

namespace Presentation
{
    /// <summary>
    ///     Interaction logic for ArchView.
    /// </summary>
    public partial class ArchView : UserControl
    {
        private readonly DTE _enviroment;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArchView" /> class.
        /// </summary>
        /// <param name="enviroment"></param>
        public ArchView(DTE enviroment)
        {
            _enviroment = enviroment;
            InitializeComponent();
        }

        public ArchView()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void GenerateDiagram(object sender, RoutedEventArgs e)
        {
            GenerateDiagram(_enviroment,BuilderSettings.Default);
        }

        public void GenerateDiagram(DTE enviroment,BuilderSettings settings)
        {
            var model = Analyser.AnalyseEnviroment(enviroment,settings);
            RenderModel(LayerMapper.TreeModelToArchViewModel(model));
        }

        public LayerView RenderNode(LayerViewModel layerModel, int depth, AdvancedColor color)
        {
            depth -= 1;
            var oldColor = color.Clone();
            var childs = new List<LayerView>();
            if (!layerModel.Anonymous)
            {
                color.L *= 1.1;
                color.S *= 1.2;
            }
            foreach (var child in layerModel.Children)
            {
                childs.Add(RenderNode(child, depth, color.Clone()));
            }

            var layerView = new LayerView(layerModel.Name, oldColor, childs, layerModel.Column, layerModel.Row,
                !layerModel.Anonymous,layerModel.Columns,layerModel.Rows);
            return layerView;
        }

        public void RenderModel(ArchViewModel model)
        {
            var colors = new Stack<AdvancedColor>(Colors.GetNColors(model.Layers.Count()));
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(RenderNode(layer, 10, colors.Pop()));
            }
        }
    }
}