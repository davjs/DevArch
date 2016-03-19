using System.IO;
using System.Linq;
using FluentAssertions;
using Logic;
using Logic.Building;
using Logic.Filtering;
using Logic.Filtering.Filters;
using Logic.Scopes;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
using ToolsMenu;
using static Tests.AssertionExtensions;
using static Tests.TestExtesions;

namespace Tests.Integration
{
    [TestClass]
    public class Integration
    {
        [TestCategory("Integration")]
        [TestMethod]
        public void FindsDependencies()
        {
            var tree = GeneratorForThisSolution
                .GenerateDiagram(DiagramDefinition.RootDefault);
            var lib = tree.Childs.WithName("Lib");
            var clients = tree.Childs.WithName("Clients");

            //Assert
            lib.DescendantNodes().Should().ContainSingle(x => x.Name == "Node").Should();
            clients.AllSubDependencies().Should().ContainSingle(x => x.Name == "DevArch");
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void SemanticTreeDoesNotContainDoubles()
        {
            var tree = SemanticTreeBuilder.FindNamespace(CurrentSolutionTree, "Logic\\SemanticTree");
            tree.DescendantNodes().Count(x => x.Name == "Node").Should().Be(1);
            tree.RelayoutBasedOnDependencies();
            tree.DescendantNodes().Count(x => x.Name == "Node").Should().Be(1);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void LogicLayerIsVertical()
        {
            var tree = SemanticTreeBuilder.FindNamespace(CurrentSolutionTree, "Logic\\Logic");
            tree.RemoveChild("DiagramDefinition");
            tree.RemoveChild("Filtering");
            tree.RemoveChild("DiagramGenerator");
            tree.RemoveChild("DiagramDefinitionParser");
            tree.RemoveChild("Common");
            tree.RemoveChild("OutputSettings");
            tree.RelayoutBasedOnDependencies();
            tree.Orientation.Should().Be(OrientationKind.Vertical);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void GeneratesWholeSolutionDiagramWithoutNamespacesWithoutCausingDuplicates()
        {
            var filters = DiagramDefinition.DefaultFilters;
            filters.Add(new RemoveContainers(true));
            var diagramDef = new DiagramDefinition("",
                new RootScope(), new OutputSettings (SlnDir + "IntegrationTests\\NoContainers.png"), filters, true,false);
            var tree = GeneratorForThisSolution.GenerateDiagram(diagramDef);
            TreeAssert.DoesNotContainDuplicates(tree);
        }

        [TestMethod]
        public void DeploysProjectSystem()
        {
            var projectFilesPath = ProjectDeployer.LocalProjectFilesPath;
            Directory.Delete(projectFilesPath,true);
            File(projectFilesPath).Should().NotExist();
            ProjectDeployer.EnsureDevArchProjectSupportExists();
            Dir(projectFilesPath).Should().Exist();
            Dir(projectFilesPath + "\\Rules").Should().Exist();
            File(projectFilesPath + "\\CustomProject.Default.props").Should().Exist();
            Directory.Delete(projectFilesPath,true);
        }
    }
}
