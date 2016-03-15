using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Tests.TestExtesions;

namespace Tests.Units.Logic.Integration
{
    [TestClass]
    public class AdvancedSolutionTests
    {
        [TestMethod]
        [TestCategory("DevArchSolution")]
        public void FindArchProjectsTest()
        {
            var dte = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.14.0");
            var testStudio = new VisualStudio(dte);

        /*var standalone = DevArchSolution.FromPath(TestSolutions.DevArchSln);
        var archProj = standalone.ArchProjects.First();
        var projectItems = archProj.GetDiagramDefinitionFiles();
        Assert.IsTrue(projectItems.Count() > 6);*/
        }
    }
}