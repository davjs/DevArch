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
            var modelGen = new DiagramFromDiagramDefinitionGenerator(solution);
            var modelDef = new DiagramDefinition("",
                new NamespaceScope {Name = @"Tests\Integration\Samples"},
                new OutputSettings {Path = solution.Directory() + @"IntegrationTests\VerticalAnonymousLayer.png" },
                new Filters {RemoveTests = false}
                );
            var tree =modelGen.GenerateDiagram(modelDef);
            BitmapRenderer.RenderTreeToBitmap(tree,modelDef.DependencyDown, modelDef.Output);
        }

        [TestCategory("PngGeneration")]
        [TestMethod]
        public void RemovesNamespaces()
        {
            var enviroment = GetDte();
            var solution = new AdvancedSolution(enviroment);
            var modelGen = new DiagramFromDiagramDefinitionGenerator(solution);
            var modelDef = new DiagramDefinition("",
                new RootScope(), 
                new OutputSettings { Path = solution.Directory() + @"IntegrationTests\WithoutNspaces.png" },
                new Filters { RemoveContainers = true,MaxDepth = 6}
                );
            //var tree = modelGen.GenerateDiagram(modelDef);
            var tree = SemanticTreeBuilder.AnalyseSolution(solution) as Node;
            tree.RemoveChild("Clients");
            ModelFilterer.ApplyFilter(ref tree, modelDef.Filters);
            BitmapRenderer.RenderTreeToBitmap(tree, modelDef.DependencyDown, modelDef.Output);
        }
    }
}