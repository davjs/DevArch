﻿using System.Linq;
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
        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAllArchDiagrams()
        {
            var enviroment = TestExtesions.Dte;
            while (enviroment.Solution == null)
            {
                enviroment = TestExtesions.Dte;
            }
            DevArch.RenderAllArchDiagramsToFiles(enviroment);
        }


        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAlignmentTest()
        {
            var enviroment = TestExtesions.Dte;
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
            var enviroment = TestExtesions.Dte;
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
            logic.RemoveChild(nameof(NamedScope));
            logic.RemoveChild("Building");
            //logic.RemoveChild("Filtering");
            var filtering = logic.Childs.WithName("Filtering");
            filtering.RemoveChild(nameof(SiblingReorderer));
            filtering.RemoveChild(nameof(ClassFilters));
            filtering.RemoveChild(nameof(NodeFilters));
            //filtering.RemoveChild(nameof(ChildrenFilter));
            logic.RemoveChild("Integration");
            logic.RemoveChild("Common");
            logic.RemoveChild(nameof(OutputSettings));
            //logic.RemoveChild(nameof(DiagramDefinition));
            //logic.RemoveChild(nameof(DiagramFromDiagramDefinitionGenerator));
            //logic.RemoveChild(nameof(DiagramDefinitionParser));
            logic.RemoveChild(nameof(Filters));
             logic.RemoveChild(nameof(NoArchProjectsFound));
            var sem = logic.Childs.WithName("SemanticTree");
            sem.RemoveChild(nameof(CircularDependencyHolderNode));
            sem.RemoveChild(nameof(VerticalSiblingHolderNode));
            sem.RemoveChild(nameof(HorizontalSiblingHolderNode));
            sem.RemoveChild(nameof(NodeExtensions));
            //sem.RemoveChild(nameof(SolutionNode));
            /* logic.RemoveChild("SemanticTree");*/

            var pres = lib.Childs.WithName("Presentation");
            pres = pres.Childs.WithName("Presentation");
            pres.RemoveChild("Coloring");
            pres.RemoveChild("Views");
            pres.RemoveChild("InvalidPathException");
            pres.RemoveChild(nameof(LayerMapper));
            var viewModels = pres.Childs.WithName("ViewModels");
            viewModels.RemoveChild(nameof(ArchViewModel));
            viewModels.RemoveChild(nameof(ArrowViewModel));
            viewModels.RemoveChild(nameof(LayerViewModel));        
            pres.RemoveChild(nameof(BitmapRenderer));

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