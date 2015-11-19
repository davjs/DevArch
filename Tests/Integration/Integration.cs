using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic;
using Logic.Building;
using Logic.Building.SemanticTree;
using Logic.Filtering;
using Logic.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Integration
{
    [TestClass]
    public class Integration
    {
        [TestMethod]
        public void FindsDependencies()
        {
            var solution = new AdvancedSolution(GetDte());
            var modelGen = new DiagramFromModelDefinitionGenerator(solution);
            var tree = modelGen.GenerateDiagram(ModelDefinition.RootDefault);
            var lib = tree.Childs.WithName("Lib");
            Assert.AreEqual(1,lib.DescendantNodes().Count(x => x.Name == "Node"));
            var clients = tree.Childs.WithName("Clients");
            var makeAllArchDiagramsInSolution = clients.DescendantNodes().WithName("MakeAllArchDiagramsInSolution");
            var deps = makeAllArchDiagramsInSolution.Dependencies.ToList();
            var dependency = clients.AllSubDependencies().Any(x => x.Name == "DevArch");
            Assert.IsNotNull(dependency);
        }


        [TestMethod]
        public void SemanticTreeDoesNotContainDoubles()
        {
            var solution = new AdvancedSolution(GetDte());
            var modelGen = new DiagramFromModelDefinitionGenerator(solution);
            var tree = SemanticTreeBuilder.AnalyseNamespace(solution, "Logic\\Building\\SemanticTree");
            tree.RemoveChild(tree.Childs.WithName("ReferenceLocationExtensions"));
            tree.RemoveChild(tree.Childs.WithName("ClassNode"));
            tree.RemoveChild(tree.Childs.WithName("ProjectNode"));
            tree.RemoveChild(tree.Childs.WithName("HorizontalSiblingHolderNode"));
            tree.RemoveChild(tree.Childs.WithName("VerticalSiblingHolderNode"));
            //tree.RemoveChild(tree.Childs.WithName("SiblingHolderNode"));
            //tree.RemoveChild(tree.Childs.WithName("CircularDependencyHolderNode"));
            //tree.RemoveChild(tree.Childs.WithName("NodeExtensions"));
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Tree"));
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Tree"));
            Assert.AreEqual(1,tree.DescendantNodes().Count(x => x.Name == "Node"));
        }


        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}
