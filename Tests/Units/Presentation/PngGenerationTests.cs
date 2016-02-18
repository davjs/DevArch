using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Lib;
using Logic;
using Logic.Building;
using Logic.Filtering;
using Logic.Filtering.Filters;
using Logic.Integration;
using Logic.Scopes;
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
            DevArch.RenderAllArchDiagramsToFiles(Dte).Wait();
        }


        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAlignmentTest()
        {
            var modelGen = new DiagramFromDiagramDefinitionGenerator(TestSolution);
            var modelDef = new DiagramDefinition("",
                new NamespaceScope {Name = @"Tests\Integration\Samples"},
                new OutputSettings (SlnDir + @"IntegrationTests\VerticalAnonymousLayer.png"),
                new List<Filter> {new RemoveTests(true) }
                );
            var tree =modelGen.GenerateDiagram(modelDef);
            BitmapRenderer.RenderTreeToBitmap(tree,modelDef.DependencyDown, modelDef.Output);
        }
    }
}