using System.Runtime.InteropServices;
using EnvDTE;
using Logic.Building;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Tests.Units.Logic
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
            var tree = Substitute.For<SolutionNode>();
            ProjectTreeBuilder.AddSolutionFoldersToTree(dte.Solution.Projects, ref tree);
            Assert.IsTrue(tree.Childs.WithName("Clients") != null);
        }
    }
}
