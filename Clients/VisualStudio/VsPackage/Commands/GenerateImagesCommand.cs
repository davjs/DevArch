using System;
using System.Globalization;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ToolsMenu.Commands
{
    internal sealed class GenerateImagesCommand : CommandBase
    {
        /// On Press
        public override void OnClick(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as _DTE;
            Lib.DevArch.RenderAllArchDiagramsToFiles(dte);
        }

        public GenerateImagesCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    internal sealed class ViewDiagramsCommand : CommandBase
    {

        /// On Press
        public override void OnClick(object sender, EventArgs e)
        {
            var message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.OnClick()", GetType().FullName);
            const string title = "View Diagrams";
            
            VsShellUtilities.ShowMessageBox(
                ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public ViewDiagramsCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
