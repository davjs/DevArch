using System;
using System.Linq;
using System.Runtime.InteropServices;
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
    }
}
