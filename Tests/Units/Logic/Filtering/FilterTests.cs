using System.Collections.Generic;
using System.Linq;
using Logic.Building;
using Logic.Filtering;
using Logic.Filtering.Filters;
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
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");
            var d = new Node("D");
            t.AddChild(a);
            a.AddChild(b);
            b.AddChild(c);
            c.AddChild(d);
            Assert.IsTrue(b.Childs.Any());
            ModelFilterer.RemoveNodesWithMoreDepthThan(t,2);
            Assert.IsTrue(!b.Childs.Any());
        }

        [TestMethod]
        public void ParentTakesOverFilteredDependencies()
        {
            var lib = new Node("Lib");
            var logic = new Node("Logic");
            var presentation = new Node("Presentation");
            var windows = new Node("Windows");
            var service = new Node("Service");
            
            var root = new Node("root");
            root.AddChild(lib);
            lib.AddChild(logic);
            lib.AddChild(presentation);
            presentation.AddChild(windows);
            logic.AddChild(service);

            windows.Dependencies.Add(service);

            root.ApplyFilters(new List<Filter>
            {
                new MaxDepth(2)
            });

            var filtered = ModelFilterer.FindSiblingDependencies(root);
            var lib2 = filtered.Childs.First();
            var pres = lib2.Childs.WithName("Presentation");

            Assert.IsTrue(pres.DependsOn(logic));
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