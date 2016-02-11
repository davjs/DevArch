//------------------------------------------------------------------------------
// <copyright file="ToolsMenuPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using ToolsMenu.Commands;

namespace DevArch
{
    // To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ToolsMenuPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class ToolsMenuPackage : Package
    {
        /// ToolsMenuPackage GUID string.
        public const string PackageGuidString = "5ce46eef-e8d5-4784-9de2-5de2263327ba";

        #region Package Members

        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        protected override void Initialize()
        {
            var serviceProvider = this;
            var commandFactory = new CommandFactory(serviceProvider);
            var guidDevarchToolsMenu = new Guid("d5a065b2-0a4e-4adc-ad08-2e4178f6ed21");

            commandFactory.AddCommand(new GenerateImagesCommand(serviceProvider), new CommandID(guidDevarchToolsMenu, 0x0105));
            //commandFactory.AddCommand(new ViewDiagramsCommand(serviceProvider), new CommandID(guidDevarchToolsMenu, 0x0106) );
            base.Initialize();
        }

        #endregion
    }
}
