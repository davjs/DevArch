using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic.Analysis.Building;
using Logic.Analysis.SemanticTree;
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
            var dte = (DTE) Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
            var tree = ProjectTreeBuilder.AddSolutionFoldersToTree(dte.Solution.Projects);
            Assert.IsTrue(tree.Childs.WithName("Clients") != null);
        }


        [TestMethod]
        [TestCategory("ProjectModel")]
        public void ContainsNestedSolutionDirectories()
        {
            var dte = (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
            var tree = ProjectTreeBuilder.AddSolutionFoldersToTree(dte.Solution.Projects);

            var clientNode = tree.Childs.WithName("Clients");
            Assert.IsNotNull(clientNode);
            var subfolder = clientNode.Childs.WithName("Folder1");
            Assert.IsNotNull(subfolder);
            var clsLibrary = subfolder.Childs.WithName("ClassLibrary1");
            Assert.IsNotNull(clsLibrary);
        }
    }
}
