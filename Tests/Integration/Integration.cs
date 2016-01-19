using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
using Logic.SemanticTree;
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
            var modelGen = new DiagramFromDiagramDefinitionGenerator(solution);
            var tree = modelGen.GenerateDiagram(DiagramDefinition.RootDefault);
            var lib = tree.Childs.WithName("Lib");
            Assert.AreEqual(1,lib.DescendantNodes().Count(x => x.Name == "Node"));
            var clients = tree.Childs.WithName("Clients");
            var dependency = clients.AllSubDependencies().Any(x => x.Name == "DevArch");
            Assert.IsNotNull(dependency);
        }

        [TestMethod]
        public void SemanticTreeDoesNotContainDoubles()
        {
            var solution = new AdvancedSolution(GetDte());
            var tree = SemanticTreeBuilder.AnalyseNamespace(solution, "Logic\\SemanticTree");
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
        }

        //TODO Copy in old version of this test from sourcecontrol
        [TestMethod]
        public void LogicDoesNotContainDoubles()
        {
            var solution = new AdvancedSolution(GetDte());
            var tree = SemanticTreeBuilder.AnalyseNamespace(solution, "Logic");
            tree = tree.Childs.First();
            tree = tree.Childs.First();
            tree.RemoveChild("Building");
            tree.RemoveChild("Common");
            tree.RemoveChild("Integration");
            tree.RemoveChild("OutputSettings");
            tree.RemoveChild("NamedScope");
            tree.RemoveChild("DocumentScope");
            tree.RemoveChild("ProjectScope");
            tree.RemoveChild("NamespaceScope");
            tree.RemoveChild("ClassScope");
            tree.RemoveChild("DiagramFromDiagramDefinitionGenerator");
            tree.RemoveChild("NoArchProjectsFound");
            var filtering = tree.DescendantNodes().WithName("Filtering");
            filtering.RemoveChild("SiblingReorderer");
            filtering.RemoveChild("PatternFinder");
            tree.RemoveChild(filtering);
            tree.AddChild(filtering.Childs.First());
            foreach (var child in tree.Childs)
            {
                //Remove those not in childs
                child.Dependencies = 
                    child.Dependencies.Intersect(tree.Childs).ToList();
            }
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "RootScope"));
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(1,tree.DescendantNodes().Count(x => x.Name == "RootScope"));
        }

        [TestMethod]
        public void LogicLayerIsVertical()
        {
            var solution = new AdvancedSolution(GetDte());
            var tree = SemanticTreeBuilder.AnalyseNamespace(solution, "Logic");
            tree = tree.Childs.First(); tree = tree.Childs.First();
            tree.RemoveChild(tree.Childs.WithName("DiagramDefinition"));
            tree.RemoveChild(tree.Childs.WithName("Filters"));
            tree.RemoveChild(tree.Childs.WithName("DiagramFromDiagramDefinitionGenerator"));
            tree.RemoveChild(tree.Childs.WithName("DiagramDefinitionParser"));
            tree.RemoveChild(tree.Childs.WithName("Common"));
            tree.RemoveChild(tree.Childs.WithName("OutputSettings"));
            tree.RemoveChild(tree.Childs.WithName("NamedScope"));
            tree.RemoveChild(tree.Childs.WithName("DocumentScope"));
            tree.RemoveChild(tree.Childs.WithName("ProjectScope"));
            tree.RemoveChild(tree.Childs.WithName("NamespaceScope"));
            tree.RemoveChild(tree.Childs.WithName("ClassScope"));
            tree.RemoveChild(tree.Childs.WithName("NoArchProjectsFound"));
            
            foreach (var child in tree.Childs)
            {
                //Remove those not in childs
                child.Dependencies =
                    child.Dependencies.Intersect(tree.Childs).ToList();
            }
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(OrientationKind.Vertical,tree.Orientation);
        }


        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}
