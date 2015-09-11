using Microsoft.VisualStudio.TestTools.UnitTesting;
using Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "GuiFacade"));
                Assert.IsTrue(tree.Childs.Any(x => x.Name == "Button"));
            }
        }

        [TestMethod]
        public void MoveDependenciesUpTest()
        {
            var root = new Node("R");
            var child = new Node("C");
            var dependency = new Node("D");
            root.Childs.Add(child);
            child.Dependencies.Add(dependency);
            var newRoot = Analyser.MoveDependenciesUp(root);
            Assert.IsTrue(newRoot.InDirectDependencies.Any(x => x.Name == "D"));
        }

    }
}