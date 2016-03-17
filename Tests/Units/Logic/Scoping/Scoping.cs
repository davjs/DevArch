using FluentAssertions;
using Logic.Building;
using Logic.Integration;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Tests.TestExtesions;

namespace Tests.Units.Logic.Scoping
{
    [TestClass]
    public class Scoping
    {
        [TestMethod]
        public void TestScoping()
        {
            var tree = SemanticTreeBuilder.AnalyseSolution(DevArchSolution.FromPath(TestSolutions.WithSolFolders));
            var project = SemanticTreeBuilder.FindProject(tree, "ClassLibrary1");
            var fooSpace = SemanticTreeBuilder.FindNamespace(tree, "ConsoleApplication1\\Foo");

            // Assert
            project.Should().BeOfType<ProjectNode>();
            fooSpace.Should().BeOfType<Node>();

            project.Name.Should().Be("ClassLibrary1");
            fooSpace.Name.Should().Be("Foo");
        }
    }
}
