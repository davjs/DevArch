using System.Linq;
using FluentAssertions;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Building.ClassTreeBuilderTests
{
    [TestClass]
    public class SemanticTreeBuilderTests
    {
        [TestMethod]
        public void LinksAncestors()
        {
            using (var testContext = TestExtesions.BuildProjectTreeFromDocuments(
                "namespace NamespaceA {class ClassOuter {}}" +
                    "namespace NamespaceA {namespace NamespaceAA {class ClassA {}}}" +
                    "namespace NamespaceA {namespace NamespaceAB {class ClassB {}}}"
                ))
            {
                //All occourances of NamespaceA creates only one node
                var tree = testContext.result;
                tree.Childs.Should().ContainSingle(x => x.Name == "NamespaceA");
            }
        }

        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void PutsNestedClassesInsideHolderClass()
        {
            using (var testContext = TestExtesions.BuildProjectTreeFromDocuments(
                "namespace NamespaceA { class ClassA { class ClassB {}}}" +
                "namespace NamespaceA { class classC {} }"
                ))
            {
                var tree = testContext.result;
                tree.DescendantNodes().WithName("ClassA")
                    .Childs.Should().ContainSingle("ClassB");
            }
        }


    }
}