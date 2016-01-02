using System.Linq;
using Logic.Building;
using Logic.Filtering;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Tests.Units.Logic
{
    [TestClass]
    public class AnalyserTests
    {
        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void ContainsProjectNames()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                fakeWorkspace.AddProject("A", LanguageNames.CSharp);
                fakeWorkspace.AddProject("B", LanguageNames.CSharp);
                var tree = new SolutionNode();
                ProjectTreeBuilder.AddProjectsToTree(fakeWorkspace.CurrentSolution,ref tree);
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "A"));
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "B"));
            }
        }

        /*
        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void IgnoresSinglePaths()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {class ClassA {}}"));
                var tree = new Tree();
                ProjectTreeBuilder.AddProjectsToTree(fakeWorkspace.CurrentSolution, ref tree);
                ModelFilterer.RemoveSinglePaths(tree);
                Assert.IsFalse(tree.Childs.Any());
            }
        }

        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void IgnoresNamespacesAllTheWayDown()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {namespace NamespaceAA {namespace NamespaceAAA {class ClassA {}}}}"));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA {namespace NamespaceAB {namespace NamespaceABB {class ClassB {}}}}"));
                var tree = new Tree();
                SemanticTreeBuilder.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                ModelFilterer.RemoveSinglePaths(tree);
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "ClassA"));
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "ClassB"));
            }
        }*/


        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void PutsNestedClassesInsideHolderClass()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA { class ClassA { class ClassB {}}}" +
                                                                                      "namespace NamespaceA { class classC {} }"));
                var tree = new SolutionNode();
                SemanticTreeBuilder.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                Assert.IsTrue(tree.DescendantNodes().WithName("ClassA").Childs.First().Name == "ClassB");
            }
        }

        [TestMethod]
        public void DependenciesAreConvertedToSiblingsIfAvailible()
        {
            var root = new Node("ROOT");
            var a = new Node("A");
            var b = new Node("B");
            var childOfB = new Node("C");
            root.AddChild(a);
            root.AddChild(b);
            b.AddChild(childOfB);
            a.Dependencies.Add(childOfB);
            var newRoot = ModelFilterer.FindSiblingDependencies(root);
            var newA = newRoot.Childs.WithName("A");
            Assert.IsNotNull(newA.SiblingDependencies.WithName("B"));
        }
    }
}