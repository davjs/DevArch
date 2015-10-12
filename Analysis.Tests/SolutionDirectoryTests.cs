using System;
using System.Linq;
using System.Runtime.InteropServices;
using Analysis.Building;
using Analysis.SemanticTree;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analysis.Tests
{
    [TestClass]
    public class SolutionDirectoryTests
    {
        [TestMethod]
        [TestCategory("ProjectModel")]
        public void ContainsSolutionDirectories()
        {
            var tree = ProjectTreeBuilder.GetSolutionFoldersTree((DTE) Marshal.
                GetActiveObject("VisualStudio.DTE.14.0"));

            Assert.IsTrue(tree.Childs.WithName("Clients") != null);
        }


        [TestMethod]
        [TestCategory("ProjectModel")]
        public void ContainsNestedSolutionDirectories()
        {
            var tree = ProjectTreeBuilder.GetSolutionFoldersTree((DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0"));

            var clientNode = tree.Childs.WithName("Clients");
            Assert.IsNotNull(clientNode);
            var subfolder = clientNode.Childs.WithName("Folder1");
            Assert.IsNotNull(subfolder);
            var clsLibrary = subfolder.Childs.WithName("ClassLibrary1");
            Assert.IsNotNull(clsLibrary);
        }
    }
}
