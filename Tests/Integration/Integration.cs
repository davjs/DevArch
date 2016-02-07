using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Lib;
using Logic;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
using Logic.Scopes;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
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
            var modelGen = new DiagramFromDiagramDefinitionGenerator(TestSolution);
            var tree = modelGen.GenerateDiagram(DiagramDefinition.RootDefault);
            var lib = tree.Childs.WithName("Lib");
            Assert.AreEqual(1,lib.DescendantNodes().Count(x => x.Name == "Node"));
            var clients = tree.Childs.WithName("Clients");
            var dependency = clients.AllSubDependencies().Any(x => x.Name == "DevArch");
            Assert.IsNotNull(dependency);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void SemanticTreeDoesNotContainDoubles()
        {
            var tree = SemanticTreeBuilder.AnalyseNamespace(TestSolution, "Logic\\SemanticTree");
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void LogicLayerIsVertical()
        {
            var tree = SemanticTreeBuilder.AnalyseNamespace(TestSolution, "Logic");
            tree = tree.Childs.First(); tree = tree.Childs.First();
            tree.RemoveChild("DiagramDefinition");
            tree.RemoveChild("Filters");
            tree.RemoveChild("DiagramFromDiagramDefinitionGenerator");
            tree.RemoveChild("DiagramDefinitionParser");
            tree.RemoveChild("Common");
            tree.RemoveChild("OutputSettings");
            tree.RemoveChild("NoArchProjectsFound");
            
            foreach (var child in tree.Childs)
            {
                //Remove those not in childs
                child.Dependencies =
                    child.Dependencies.Intersect(tree.Childs).ToList();
            }
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(OrientationKind.Vertical,tree.Orientation);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void GeneratesWholeSolutionDiagramWithoutNamespacesWithoutCausingDuplicates()
        {
            var diagramGen = new DiagramFromDiagramDefinitionGenerator(TestSolution);
            var diagramDef = new DiagramDefinition("",
                new RootScope(), new OutputSettings {Path = SlnDir + "IntegrationTests\\NoContainers.png"}, new Filters { RemoveContainers = true }, true, false);
            var tree = diagramGen.GenerateDiagram(diagramDef);
            BitmapRenderer.RenderTreeToBitmap(tree, diagramDef.DependencyDown, diagramDef.Output, diagramDef.HideAnonymousLayers);
            TreeAssert.DoesNotContainDuplicates(tree);
        }
    }
}
