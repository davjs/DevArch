using System.Collections.Generic;
using Logic;
using Logic.Filtering.Filters;
using Logic.Scopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
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
            Lib.DevArch.RenderAllArchDiagramsToFiles(TestStudio).Wait();
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