using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using Logic.Filtering;
using Logic.Integration;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

// ReSharper disable InconsistentNaming

namespace Tests.Units.Logic.Filtering
{
    [TestClass]
    public class NotProduceDuplicateTests
    {
        [TestCategory("SiblingOrder.DuplicateTests")]
        [TestMethod]
        public void RemovesNodesFromToBeGroupedThatArePartOfNestedGroup()
        {
            //In the following test, diagramsymbol will be added in the nested call when grouping arrowView and archView
            //Allthough only arrowView and archView will be sent as input to the nested call, the nested call will also add diagramSymbol, since both of them depend on diagramSymbol
            Node node = new Node(nameof(node));
            Node diagramSymbol = new Node(nameof(diagramSymbol));
            Node CircularDependencyHolder = new Node(nameof(CircularDependencyHolder)) { SiblingDependencies = {node} };
            Node archView = new Node(nameof(archView)) {SiblingDependencies = {diagramSymbol}};
            Node arrowView = new Node(nameof(arrowView)) { SiblingDependencies = { diagramSymbol } };
            Node layerMapper = new Node(nameof(layerMapper)) { SiblingDependencies = {node,archView,diagramSymbol,arrowView}};
            Node bitmapRenderer = new Node(nameof(bitmapRenderer)) {SiblingDependencies = {node,archView,layerMapper}};
            
            var newList = SiblingReorderer.RegroupSiblingNodes(new List<Node>
            {
                node,diagramSymbol,CircularDependencyHolder,archView,arrowView,layerMapper,bitmapRenderer
            }).ToList();

            // bitmaprender
            // layermapper
            // arrow arch
            // diagramsybol      circulardeps
            //              node

            var tree = new Node("tree");
            tree.SetChildren(newList);
            var allNodes = tree.DescendantNodes().Where(x => !string.IsNullOrEmpty(x.Name));

            var dups = allNodes.GroupBy(x => x)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key)
                        .ToList();


            DiagramFromDiagramDefinitionGenerator.ReverseTree(tree);

            var solution = new AdvancedSolution(TestExtesions.Dte);
            BitmapRenderer.RenderTreeToBitmap(tree, true, new OutputSettings { Path = solution.Directory() + @"IntegrationTests\Analysis.png" });

            if (dups.Any())
                throw new AssertFailedException(dups.First().ToString() + allNodes.Count());
        }


        [TestCategory("SiblingOrder.DuplicateTests")]
        [TestMethod]
        public void RemovesNodesFromNextTargetGroupedThatArePartOfNestedGroup()
        {
            //Produes duplicate RootScope
            Node rootScope = new Node(nameof(rootScope));
            Node diagramSymbol = new Node(nameof(diagramSymbol));
            Node layerMapper = new Node(nameof(layerMapper)) { SiblingDependencies = { diagramSymbol } };
            Node diagramDefiniton = new Node(nameof(diagramDefiniton)) { SiblingDependencies = { rootScope } };
            Node diagramDefinitionParser = new Node(nameof(diagramDefinitionParser)) { SiblingDependencies = { diagramDefiniton,rootScope } };

            Node smallClassFilter= new Node(nameof(smallClassFilter));
            Node patternFinder = new Node(nameof(patternFinder));
            Node modelFilterer = new Node(nameof(modelFilterer)) { SiblingDependencies = { smallClassFilter,patternFinder }};
            Node diagramDefinitonGenerator = new Node(nameof(diagramDefinitonGenerator))
            {
                SiblingDependencies = { diagramDefinitionParser, diagramDefiniton,rootScope,modelFilterer }
            };

            var newList = SiblingReorderer.RegroupSiblingNodes(new List<Node>
            {
                rootScope,patternFinder,smallClassFilter,diagramSymbol,diagramDefiniton,diagramDefinitonGenerator,diagramDefinitionParser,layerMapper,modelFilterer
            }).ToList();
            

            var tree = new Node("tree");
            tree.SetChildren(newList);
            var allNodes = tree.DescendantNodes().Where(x => !string.IsNullOrEmpty(x.Name));

            var dups = allNodes.GroupBy(x => x)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key)
                        .ToList();


            DiagramFromDiagramDefinitionGenerator.ReverseTree(tree);

            var solution = new AdvancedSolution(TestExtesions.Dte);
            BitmapRenderer.RenderTreeToBitmap(tree, true, new OutputSettings { Path = solution.Directory() + @"IntegrationTests\Analysis2.png" });

            if (dups.Any())
                throw new AssertFailedException(dups.First().ToString() + allNodes.Count());
        }
    }
}
