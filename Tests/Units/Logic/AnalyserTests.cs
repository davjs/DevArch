using System.Collections.Generic;
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


        [TestMethod]
        public void FindsIndirectSiblingDependencies()
        {
            var siblings = OrderingTestFactory.CreateNodeList(@"
            A -> B
            B -> C
            C -> D
            D ->            
            ");

            ModelFilterer.SetupIndirectSiblingDeps(siblings);
            var A = siblings.WithName("A");
            var B = siblings.WithName("B");
            var C = siblings.WithName("C");
            var D = siblings.WithName("D");

            Assert.IsTrue(A.IndirectSiblingDependencies.SetEquals(new HashSet<Node>{
                B,C,D
            }));
            
            Assert.IsTrue(B.IndirectSiblingDependencies.SetEquals(new HashSet<Node>{
                C,D
            }));
            
            Assert.IsTrue(C.IndirectSiblingDependencies.SetEquals(new HashSet<Node>{
                D
            }));
        }
    }
}