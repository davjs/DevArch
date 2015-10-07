using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Analysis.Building;
using Analysis.SemanticTree;
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
            var tree = Analyser.AnalyseEnviroment((DTE) Marshal.
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
                var tree = new Tree();
                ProjectTreeBuilder.AddProjectsToTree(fakeWorkspace.CurrentSolution,ref tree);
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
                var tree = new Tree();
                ProjectTreeBuilder.AddProjectsToTree(fakeWorkspace.CurrentSolution, ref tree);
                tree = Analyser.RemoveSinglePaths(tree);
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
                Analyser.AnalyzeSolutionToTree(fakeWorkspace.CurrentSolution, ref tree,BuilderSettings.Default);
                tree = Analyser.RemoveSinglePaths(tree);
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
                var tree = new Tree();
                Analyser.AnalyzeSolutionToTree(fakeWorkspace.CurrentSolution,ref tree, BuilderSettings.Default);
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
                Analyser.AnalyzeSolutionToTree(fakeWorkspace.CurrentSolution, ref tree, BuilderSettings.Default);
                Assert.IsNotNull(tree.Childs.WithName("GuiFacade"));
                Assert.IsNotNull(tree.Childs.WithName("Button"));
            }
        }


        [TestMethod]
        [TestCategory("ModelBuilder")]
        public void PutsNestedClassesInsideHolderClass()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA { class ClassA { class ClassB {}}}" +
                                                                                      "namespace NamespaceA { class classC {} }"));
                var tree = new Tree();
                Analyser.AnalyzeSolutionToTree(fakeWorkspace.CurrentSolution, ref tree, BuilderSettings.Default);
                Assert.IsTrue(tree.Childs.First().Name == "ClassB");
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
    }
}