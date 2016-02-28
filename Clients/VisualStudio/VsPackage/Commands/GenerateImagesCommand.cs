using System;
using System.Linq;
using DevArch;
using EnvDTE;
using Logic.Integration;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;

namespace ToolsMenu.Commands
{
    internal sealed class GenerateImagesCommand : CommandBase
    {
        private readonly VisualStudioWorkspace _vsWorkspace;
        /// On Press
        public override async void OnClick(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE;
            var solution = _vsWorkspace.CurrentSolution;
            try
            {
                var vs = new VisualStudio(dte, solution);
                await Lib.DevArch.RenderAllArchDiagramsToFiles(vs);
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
        public GenerateImagesCommand(ToolsMenuPackage serviceProvider, VisualStudioWorkspace vsWorkspace) : base(serviceProvider)
        {
            _vsWorkspace = vsWorkspace;
        }
    }
}
