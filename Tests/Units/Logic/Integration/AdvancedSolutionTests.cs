using System.Linq;
using FluentAssertions;
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
            var standalone = ThisDevArchSolution;
            var archProj = standalone.ArchProjects.First();
            var projectItems = archProj.GetDiagramDefinitionFiles();
            projectItems.Count().Should().BeGreaterThan(6);
        }
    }
}