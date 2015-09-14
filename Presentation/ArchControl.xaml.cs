//------------------------------------------------------------------------------
// <copyright file="ArchControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
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

        public void RenderModel(ITreeViewModel model)
        {
            foreach (var child in model.Childs)
            {
                Stackpanel.Children.Add(new Button{ Content = child.Name });
            }
        }

    }
}