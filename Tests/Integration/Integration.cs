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
            var tree = SemanticTreeBuilder.AnalyseNamespace(solution, "Logic\\SemanticTree");
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Tree"));
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Tree"));
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
            tree.RemoveChild(tree.Childs.WithName("Building"));
            tree.RemoveChild(tree.Childs.WithName("Common"));
            tree.RemoveChild(tree.Childs.WithName("Integration"));
            //tree.RemoveChild(tree.Childs.WithName("ModelDefinition"));
            //tree.RemoveChild(tree.Childs.WithName("Filters"));
            tree.RemoveChild(tree.Childs.WithName("OutputSettings"));
            tree.RemoveChild(tree.Childs.WithName("NamedScope"));
            tree.RemoveChild(tree.Childs.WithName("DocumentScope"));
            tree.RemoveChild(tree.Childs.WithName("ProjectScope"));
            tree.RemoveChild(tree.Childs.WithName("NamespaceScope"));
            tree.RemoveChild(tree.Childs.WithName("ClassScope"));
            tree.RemoveChild(tree.Childs.WithName("DiagramFromModelDefinitionGenerator"));
            //tree.RemoveChild(tree.Childs.WithName("ModelDefinitionParser"));
            tree.RemoveChild(tree.Childs.WithName("NoArchProjectsFound"));
            var filtering = tree.DescendantNodes().WithName("Filtering");
            filtering.RemoveChild("SiblingReorderer");
            filtering.RemoveChild("PatternFinder");
            tree.RemoveChild(filtering);
            tree.AddChild(filtering.Childs.First());
            //tree.RemoveChild(tree.Childs.WithName("Filtering"));
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
            tree.RemoveChild(tree.Childs.WithName("ModelDefinition"));
            tree.RemoveChild(tree.Childs.WithName("Filters"));
            tree.RemoveChild(tree.Childs.WithName("DiagramFromModelDefinitionGenerator"));
            tree.RemoveChild(tree.Childs.WithName("ModelDefinitionParser"));
            /*var building = tree.Childs.WithName("Building");
            tree.RemoveChild(building);
            tree.AddChild(building.Childs.First());
            */
            tree.RemoveChild(tree.Childs.WithName("Common"));
            tree.RemoveChild(tree.Childs.WithName("OutputSettings"));
            tree.RemoveChild(tree.Childs.WithName("NamedScope"));
            tree.RemoveChild(tree.Childs.WithName("DocumentScope"));
            tree.RemoveChild(tree.Childs.WithName("ProjectScope"));
            tree.RemoveChild(tree.Childs.WithName("NamespaceScope"));
            tree.RemoveChild(tree.Childs.WithName("ClassScope"));
            tree.RemoveChild(tree.Childs.WithName("NoArchProjectsFound"));
            

            /*var filtering = tree.DescendantNodes().WithName("Filtering");
            tree.RemoveChild(filtering);
            tree.AddChild(filtering.Childs.WithName("ModelFilterer"));
            */
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
