using System;
using System.Globalization;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace DevArch.Commands
{
    internal sealed class GenerateImagesCommand : CommandBase
    {
        private readonly IComponentModel _componentModel;

        /// On Press
        public override async void OnClick(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as _DTE;

            var workspace = _componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();
            var solution = workspace.CurrentSolution;

            //#if DEBUG
            await Task.Run(() => Lib.DevArch.RenderAllArchDiagramsToFiles(dte, solution));
/*#else
            try
            {
                await Task.Run(() => Lib.DevArch.RenderAllArchDiagramsToFiles(dte));
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
#endif*/
        }


        public GenerateImagesCommand(IServiceProvider serviceProvider, IComponentModel componentModel) : base(serviceProvider)
        {
            _componentModel = componentModel;
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
