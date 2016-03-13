using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Logic;
using Logic.Filtering.Filters;
using Logic.Integration;
using Logic.Scopes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Building
{
    [TestClass]
    public class DiagramGeneratorTest
    {
        [TestMethod]
        public void CreatesNewTreesForEachRequest()
        {
            var generator = 
                new DiagramGenerator(DevArchSolution.FromPath(TestExtesions.TestSolutions.WithSolFolders));
            var def1 = new DiagramDefinition("",new RootScope(), new OutputSettings(""),
                new List<Filter>{new MaxDepth(1)});
            var def2 = new DiagramDefinition("", new RootScope(), new OutputSettings(""),
                new List<Filter> { new MaxDepth(2) });

            var diagram1 = generator.GenerateDiagram(def1);
            var diagram2 = generator.GenerateDiagram(def2);

            //Assert
            diagram1.Should().NotBeSameAs(diagram2);
            diagram1.Height().Should().Be(1);
            diagram2.Height().Should().Be(2);
        }
    }
}
