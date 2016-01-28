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
using Presentation.ViewModels;

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
            var modelDef = new DiagramDefinition("",
                new RootScope(), 
                new OutputSettings { Path = solution.Directory() + @"IntegrationTests\WithoutNspaces.png" },
                new Filters { RemoveContainers = true,MaxDepth = 6}
                );
            //var tree = modelGen.GenerateDiagram(modelDef);
            var tree = SemanticTreeBuilder.AnalyseSolution(solution) as Node;
            tree.RemoveChild("Clients");
            var lib = tree.Childs.WithName("Lib");
            lib.RemoveChild("Lib");
            var logic = lib.Childs.WithName("Logic");
            logic = logic.Childs.WithName("Logic");
            logic.RemoveChild("NamespaceScope");
            logic.RemoveChild("DocumentScope");
            logic.RemoveChild("ClassScope");
            logic.RemoveChild("ProjectScope");
            logic.RemoveChild("RootScope");
            logic.RemoveChild(nameof(NamedScope));
            logic.RemoveChild("Building");
            logic.RemoveChild("Filtering");
            logic.RemoveChild(nameof(OutputSettings));
            logic.RemoveChild(nameof(DiagramDefinition));
            logic.RemoveChild(nameof(DiagramFromDiagramDefinitionGenerator));
            logic.RemoveChild(nameof(DiagramDefinitionParser));

            var semanticTree = logic.Childs.WithName("SemanticTree");
            semanticTree.RemoveChild(nameof(UniqueEntity));
            semanticTree.RemoveChild(nameof(NodeExtensions));
            semanticTree.RemoveChild(nameof(ProjectNode));
            semanticTree.RemoveChild(nameof(SiblingHolderNode));
            semanticTree.RemoveChild(nameof(SolutionNode));
            semanticTree.RemoveChild(nameof(VerticalSiblingHolderNode));
            semanticTree.RemoveChild(nameof(HorizontalSiblingHolderNode));
            semanticTree.RemoveChild(nameof(ClassNode));
            //logic.RemoveChild("SemanticTree");

            var pres = lib.Childs.WithName("Presentation");
            pres = pres.Childs.WithName("Presentation");
            pres.RemoveChild("Coloring");
            pres.RemoveChild("Views");
            var viewModels = pres.Childs.WithName("ViewModels");
            viewModels.RemoveChild(nameof(LayerViewModel));            
            //pres.RemoveChild(nameof(BitmapRenderer));

            ModelFilterer.ApplyFilter(ref tree, modelDef.Filters);
            DiagramFromDiagramDefinitionGenerator.ReverseTree(tree);

            var allNodes = tree.DescendantNodes().Where(x => !string.IsNullOrEmpty(x.Name));

            var dups = allNodes.GroupBy(x => x)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key)
                        .ToList();

            BitmapRenderer.RenderTreeToBitmap(tree, modelDef.DependencyDown, modelDef.Output);

            if (dups.Any())
                throw new AssertFailedException(dups.First().ToString() + allNodes.Count());
        }
    }
}