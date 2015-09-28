//------------------------------------------------------------------------------
// <copyright file="ArchitechtureToolWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Composition;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Package
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("0f613c0d-a0cf-4e28-a5b2-f7ed3a35ea78")]
    public class ArchitechtureToolWindow : ToolWindowPane
    {

        public static DTE Enviro;
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchitechtureToolWindow"/> class.
        /// </summary>
        public ArchitechtureToolWindow() : base(null)
        {
            Caption = "ArchitechtureToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            //TODO: Enable reopening with different project
            var toolWindow = new Presentation.ArchControl(Enviro);
            toolWindow.GenerateDiagram(Enviro);
            Content = toolWindow;

        }
    }
}
