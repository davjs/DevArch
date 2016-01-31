using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Lib;
using Logic;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
using Presentation.Coloring;
using Presentation.Coloring.ColoringAlgorithms;
using Presentation.ViewModels;
using Presentation.Views;
using static Tests.TestExtesions;

namespace Tests.Units.Presentation
{
    [TestClass]
    public class PngGenerationTests
    {
        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAllArchDiagrams()
        {
            DevArch.RenderAllArchDiagramsToFiles(Dte);
        }


        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAlignmentTest()
        {
            var modelGen = new DiagramFromDiagramDefinitionGenerator(TestSolution);
            var modelDef = new DiagramDefinition("",
                new NamespaceScope {Name = @"Tests\Integration\Samples"},
                new OutputSettings {Path = SlnDir + @"IntegrationTests\VerticalAnonymousLayer.png" },
                new Filters {RemoveTests = false}
                );
            var tree =modelGen.GenerateDiagram(modelDef);
            BitmapRenderer.RenderTreeToBitmap(tree,modelDef.DependencyDown, modelDef.Output);
        }

        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GeneratesWholeSolutionDiagramWithoutNamespacesWithoutCausingDuplicates()
        {
            var modelDef = new DiagramDefinition("",
                new RootScope(), 
                new OutputSettings { Path = SlnDir + @"IntegrationTests\WithoutNspaces.png" },
                new Filters { RemoveContainers = true}
                );

            var tree = SemanticTreeBuilder.AnalyseSolution(TestSolution) as Node;

            ModelFilterer.ApplyFilter(ref tree, modelDef.Filters);
            DiagramFromDiagramDefinitionGenerator.ReverseTree(tree);

            var allNodes = tree.DescendantNodes().Where(x => !string.IsNullOrEmpty(x.Name));

            var dups = allNodes.GroupBy(x => x)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key)
                        .ToList();

            if (dups.Any())
                throw new AssertFailedException(dups.First().ToString() + allNodes.Count());
        }
    }
}