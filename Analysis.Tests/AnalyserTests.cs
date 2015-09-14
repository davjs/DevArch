using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Analysis.Tests
{
    [TestClass]
    public class AnalyserTests
    {
        [TestMethod]
        public void Analyse()
        {
            var tree = Analyser.AnalyseEnviroment((DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0"));
        }

        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void ContainsProjectNames()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                fakeWorkspace.AddProject("A", LanguageNames.CSharp);
                fakeWorkspace.AddProject("B", LanguageNames.CSharp);
                var tree = Analyser.AnalyzeSolution(fakeWorkspace.CurrentSolution);
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "A"));
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "B"));
            }
        }

        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void IgnoresSinglePaths()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {class ClassA {}}"));
                var tree = Analyser.AnalyzeSolution(fakeWorkspace.CurrentSolution);
                Assert.IsFalse(tree.Childs.Any());
            }
        }

        /*[TestMethod]
        [TestCategory("ModelBuilder")]
        public void IgnoresFirstNamespaceLevel()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {namespace NamespaceB {class ClassA {}}}"));
                var tree = Analyser.AnalyzeSolution(fakeWorkspace.CurrentSolution);
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "NamespaceB"));
            }
        }*/

        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void IgnoresNamespacesAllTheWayDown()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {namespace NamespaceAA {namespace NamespaceAAA {class ClassA {}}}}"));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA {namespace NamespaceAB {namespace NamespaceABB {class ClassB {}}}}"));
                var tree = Analyser.AnalyzeSolution(fakeWorkspace.CurrentSolution);
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "ClassA"));
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "ClassB"));
            }
        }

        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void FindsClassesOnDifferentLevels()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From("namespace NamespaceA {namespace GUI {class GuiFacade {}}}"));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA {namespace GUI {namespace Buttons {class Button {}}}}"));
                var tree = Analyser.AnalyzeSolution(fakeWorkspace.CurrentSolution);
                Assert.IsNotNull(tree.Childs.WithName("GuiFacade"));
                Assert.IsNotNull(tree.Childs.WithName("Button"));
            }
        }

        [TestMethod]
        public void DependenciesAreConvertedToSiblingsIfAvailible()
        {
            var root = new Node("R");
            var a = new Node("A");
            var b = new Node("B");
            var childOfB = new Node("C");
            root.AddChild(a);
            root.AddChild(b);
            b.AddChild(childOfB);
            a.Dependencies.Add(childOfB);
            var newRoot = Analyser.FindSiblingDependencies(root);
            var newA = newRoot.Childs.WithName("A");
            Assert.IsNotNull(newA.SiblingDependencies.WithName("B"));
        }

        [TestMethod]
        public void SiblingsAreOrderedByDependency()
        {
            var root = new Tree();
            var a = new Node("A");
            var b = new Node("B");
            root.AddChild(a);
            root.AddChild(b);
            Assert.IsTrue(root.Childs.SequenceEqual(new List<Node> { a, b }));
            Assert.IsFalse(root.Childs.SequenceEqual(new List<Node> { b, a }));
            a.SiblingDependencies.Add(b);
            root.UpdateChildren(Analyser.OrderChildsBySiblingsDependencies(root.Childs));
            Assert.IsTrue(root.Childs.SequenceEqual(new List<Node> { b, a }));
            Assert.IsFalse(root.Childs.SequenceEqual(new List<Node> { a, b }));
        }
    }
}