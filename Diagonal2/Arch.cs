﻿//------------------------------------------------------------------------------
// <copyright file="Arch.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Diagonal2
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
    public class Arch : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Arch"/> class.
        /// </summary>
        public Arch() : base(null)
        {
            Caption = "Arch";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new ArchControl();
        }
    }
}
