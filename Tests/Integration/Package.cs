using System;
using EnvDTE;
using Logic.Integration;
using Microsoft.CodeAnalysis.Host;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.IntegrationTestLibrary;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Tests.Integration
{
    [TestClass]
    public class Package
    {
        /*
        [TestMethod]
        [HostType("VS IDE")]
        public void PackageLoadTest()
        {
            //UIThreadInvoker.Invoke((ThreadInvoker) delegate
            //{
            var shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof (SVsShell)) as IVsShell;
            Assert.IsNotNull(shellService);
            
            IVsPackage package;
            var guid = new Guid(ToolsMenuPackage.PackageGuidString);
            Assert.IsTrue(0 == shellService.LoadPackage(ref guid, out package));
            
            Assert.IsNotNull(package, "Package failed to load");

            var testUtils = new TestUtils();
            //testUtils.CreateEmptySolution("testDirr","TestSln");
            //DevArchProject\\1033\\DevArchProject.zip|FrameworkVersion=4.5.2
            //"Extensibility\\1033\\VSIXProject.zip|FrameworkVersion=4.5"
            testUtils.CreateProjectFromTemplate("newProj", "DevArch", "CSharp");
//            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void LoadPackageInline()
        {
            //var serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider) TestExtesions.Dte);
            var serviceProvider = VsIdeTestHostContext.ServiceProvider;
            var shellService = serviceProvider.GetService(typeof(SVsShell)) as IVsShell;
            var dte = serviceProvider.GetService(typeof(DTE)) as DTE;
            Assert.IsNotNull(shellService);

            IVsPackage package;
            var guid = new Guid(ToolsMenuPackage.PackageGuidString);
            Assert.IsTrue(0 == shellService.LoadPackage(ref guid, out package));
            Assert.IsTrue(shellService.IsPackageLoaded(ref guid, out package) == VSConstants.S_OK);
            
            var menuPack = package as ToolsMenuPackage;
            Assert.IsNotNull(menuPack?.Workspace);
            TestUtils.OpenSolution("C:\\dev\\Repos\\DevArch\\DevArch.sln");

            Lib.DevArch.RenderAllArchDiagramsToFiles(new VisualStudio(dte,menuPack.Workspace.CurrentSolution)).Wait();
            Assert.IsNotNull(dte);
            Assert.IsNotNull(menuPack.Workspace.CurrentSolution);
        }*/
    }
}