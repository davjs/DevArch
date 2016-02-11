using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ToolsMenu.Commands
{
    internal sealed class GenerateImagesCommand : CommandBase
    {
        /// On Press
        public override async void OnClick(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as _DTE;
            try
            {
                await Lib.DevArch.RenderAllArchDiagramsToFiles(dte);
            }
            catch (Exception exception)
            {
                VsShellUtilities.ShowMessageBox(
                    ServiceProvider,
                    exception.Message,
                    "Unable to generate diagrams",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }


        public GenerateImagesCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
    }
}
