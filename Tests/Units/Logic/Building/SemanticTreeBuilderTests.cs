using System.Linq;
using Logic.Building;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Tests.Units.Logic.Building
{
    [TestClass]
    public class SemanticTreeBuilderTests
    {
        [TestMethod]
        public void LinksAncestors()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs",
                    SourceText.From(
                        "namespace NamespaceA {class ClassOuter {}}" +
                        "namespace NamespaceA {namespace NamespaceAA {class ClassA {}}}" +
                        "namespace NamespaceA {namespace NamespaceAB {class ClassB {}}}"));
                var tree = Substitute.For<SolutionNode>();
                ProjectTreeBuilder.AddDocumentsToProjects(fakeWorkspace.CurrentSolution, ref tree);
                ClassTreeBuilder.AddClassesInProjectsToTree(tree);
                Assert.AreEqual(1,tree.Childs.First().Childs.Count);
            }
        }
    }
}