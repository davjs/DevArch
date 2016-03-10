using System;
using Logic;
using Logic.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Building
{
    [TestClass]
    public class DiagramGeneratorTest
    {
        [TestMethod]
        public void CreatesNewTreesForEachRequest()
        {
            DiagramFromDiagramDefinitionGenerator generator = new DiagramFromDiagramDefinitionGenerator(new DevArchSolution());
        }
    }
}
