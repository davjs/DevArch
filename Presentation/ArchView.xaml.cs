//------------------------------------------------------------------------------
// <copyright file="ArchView.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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
            GenerateDiagram(_enviroment);
        }

        public void GenerateDiagram(DTE enviroment)
        {
            var model = Analyser.AnalyseEnviroment(enviroment);
            RenderModel(LayerMapper.TreeModelToArchViewModel(model));
        }

        public LayerView RenderNode(LayerViewModel layerModel, int depth, Color color)
        {
            var childs =
                //depth > 0 ? 
                layerModel.Children.Select(n => RenderNode(n, depth, color))
                //    : new List<LayerView>()
                ;
            var layerView = new LayerView(layerModel.Name, color, childs, layerModel.Column, layerModel.Row,
                !layerModel.Anonymous,layerModel.Columns,layerModel.Rows);
            return layerView;
        }

        public void RenderModel(ArchViewModel model)
        {
            var c = Color.FromRgb(0, 43, 54);
            foreach (var layer in model.Layers)
            {
                MasterPanel.Children.Add(RenderNode(layer, 10, c));
            }
        }
    }
}