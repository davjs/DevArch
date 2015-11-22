using System.Runtime.InteropServices;
using EnvDTE;
using Lib;
using Logic;
using Logic.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

namespace Tests.Units.Presentation
{
    [TestClass]
    public class PngGenerationTests
    {
        private static DTE GetDte()
        {
            return (DTE) Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }

        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAllArchDiagrams()
        {
            var enviroment = GetDte();
            while (enviroment.Solution == null)
            {
                enviroment = GetDte();
            }
            DevArch.RenderAllArchDiagramsToFiles(enviroment);
        }


        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAlignmentTest()
        {
            var enviroment = GetDte();
            var solution = new AdvancedSolution(enviroment);
            var modelGen = new DiagramFromModelDefinitionGenerator(solution);
            var modelDef = new ModelDefinition("",
                new NamespaceScope {Name = @"Tests\Integration\Samples"},
                new OutputSettings {Path = @"IntegrationTests\VerticalAnonymousLayer.png"},
                new Filters {RemoveTests = false}
                );
            var tree =modelGen.GenerateDiagram(modelDef);
            var model = LayerMapper.TreeModelToArchViewModel(tree);
        }
    }
}