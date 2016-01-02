using System.Linq;
using Logic.Building;
using Logic.Filtering;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Filtering
{
    [TestClass]
    public class FilterTests
    {
        [TestMethod()]
        public void RemoveNodesWithMoreDepthThanTest()
        {
            var t = new Node("ROOT");
            var A = new Node("A");
            var B = new Node("B");
            var C = new Node("C");
            var D = new Node("D");
            t.AddChild(A);
            A.AddChild(B);
            B.AddChild(C);
            C.AddChild(D);
            Assert.IsTrue(B.Childs.Any());
            ModelFilterer.RemoveNodesWithMoreDepthThan(t,2);
            Assert.IsTrue(!B.Childs.Any());
        }
        /*
        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void FindsClassesOnDifferentLevels()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {namespace GUI {class GuiFacade {}}}"));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA {namespace GUI {namespace Buttons {class Button {}}}}"));
                var tree = new Tree();
                SemanticTreeBuilder.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution,ref tree);
                ModelFilterer.RemoveSinglePaths(tree);
                Assert.IsNotNull(tree.Childs.WithName("GuiFacade"));
                Assert.IsNotNull(tree.Childs.WithName("Button"));
            }
        }

        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void FindsClassesOnDifferentLevels2()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {namespace GUI {class GuiFacade {}}}"));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA {namespace GUI {namespace Buttons { namespace Purple {class Button {}}}}}"));
                var tree = new Tree();
                SemanticTreeBuilder.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                ModelFilterer.RemoveSinglePaths(tree);
                Assert.IsNotNull(tree.Childs.WithName("GuiFacade"));
                Assert.IsNotNull(tree.Childs.WithName("Button"));

            }
        }*/
    }
}