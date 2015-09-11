//------------------------------------------------------------------------------
// <copyright file="ArchControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Analysis;
using EnvDTE;
using static System.Globalization.CultureInfo;
using static System.String;

namespace Diagonal2
{
    /// <summary>
    /// Interaction logic for ArchControl.
    /// </summary>
    public partial class ArchControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchControl"/> class.
        /// </summary>
        public ArchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void GenerateDiagram(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Format(CurrentUICulture, $"Invoked '{this}'"),"Arch");

            var dte2 = GetDte();
            var model = Analyser.AnalyseEnviroment(dte2);
            RenderModel(model);
        }

        public void RenderModel(ITreeViewModel model)
        {
            foreach (var child in model.Childs)
            {
                Stackpanel.Children.Add(new Button{ Content = child.Name });
            }
        }

        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}