using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic;
using Logic.Building;
using Logic.Building.SemanticTree;
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
        

        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}
