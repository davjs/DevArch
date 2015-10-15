//------------------------------------------------------------------------------
// <copyright file="ArchCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Package
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ArchCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        private const int CommandId = 258;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        private static readonly Guid CommandSet = new Guid("789b5525-1334-4403-8382-b0adb17f3ff7");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Microsoft.VisualStudio.Shell.Package _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ArchCommand(Microsoft.VisualStudio.Shell.Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            _package = package;

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService == null) return;
            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(ShowToolWindow, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => _package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Microsoft.VisualStudio.Shell.Package package)
        {
            new ArchCommand(package);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = _package.FindToolWindow(typeof(ArchitechtureToolWindow), 0, true);
            
            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
