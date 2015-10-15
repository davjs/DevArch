using Logic.Analysis;
using Logic.Analysis.SemanticTree;
using Logic.Filtering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analysis.Tests.Filtering
{
    [TestClass]
    public class FilterTests
    {
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
                Analyser.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution,ref tree);
                tree = ModelFilterer.RemoveSinglePaths(tree);
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
                Analyser.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                tree = ModelFilterer.RemoveSinglePaths(tree);
                Assert.IsNotNull(tree.Childs.WithName("GuiFacade"));
                Assert.IsNotNull(tree.Childs.WithName("Button"));

            }
        }
    }
}