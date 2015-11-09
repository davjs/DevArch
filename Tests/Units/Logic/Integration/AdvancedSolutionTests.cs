using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Integration
{
    [TestClass]
    public class AdvancedSolutionTests
    {
        [TestMethod]
        [TestCategory("AdvancedSolution")]
        public void FindArchProjectsTest()
        {
            var sln = new AdvancedSolution(GetDte());
            var archProj = sln.FindArchProjects().First();
            var projectItems = archProj.GetAllProjectItems();
            Assert.IsTrue(projectItems.Count() > 6);
        }


        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}