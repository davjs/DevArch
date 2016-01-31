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
        [TestCategory("AdvancedSolution")]
        public void FindArchProjectsTest()
        {
            var archProj = TestSolution.FindArchProjects().First();
            var projectItems = archProj.GetAllProjectItems();
            Assert.IsTrue(projectItems.Count() > 6);
        }
        
    }
}