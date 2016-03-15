using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
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
        public void PutsNestedClassesInsideHolderClass()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                var doc = fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("namespace NamespaceA { class ClassA { class ClassB {}}}" +
                                                                                      "namespace NamespaceA { class classC {} }"));
                var projectA = new ProjectNode(null);
                projectA.Name.Returns("ProjectA");
                projectA.Documents = new List<Document> {doc};
                var tree = new SolutionNode();
                tree.AddChild(projectA);
                ClassTreeBuilder.AddClassesInProjectsToTree(tree);
                tree.DescendantNodes().WithName("ClassA")
                    .Childs.Should().ContainSingle(x => x.Name == "ClassB");
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

        [TestCategory("IndirectSiblingDependencies")]
        [TestMethod]
        public void FindsIndirectSiblingDependencies()
        {
            var root = OrderingTestFactory.CreateNodeList(@"
            A -> B
            B -> C
            C -> D
            D ->            
            ");
            
            var A = root.WithName("A");
            var B = root.WithName("B");
            var C = root.WithName("C");
            var D = root.WithName("D");

            Assert.IsTrue(A.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                B,C,D
            }));
            
            Assert.IsTrue( B.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                C,D
            }));
            
            Assert.IsTrue(C.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                D
            }));
        }

        [TestCategory("IndirectSiblingDependencies")]
        [TestMethod]
        public void CirclularDepsDoesNotCauseIndirectSiblingCalcToHang()
        {
            var nodes = OrderingTestFactory.CreateNodeList(@"
            A -> B
            B -> C
            C -> D
            D -> C
            ");
            
            var A = nodes.WithName("A");
            var B = nodes.WithName("B");
            var C = nodes.WithName("C");
            var D = nodes.WithName("D");

            Assert.IsTrue(A.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                B,C,D
            }));

            Assert.IsTrue(B.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                C,D
            }));

            Assert.IsTrue(C.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                D
            }));

            Assert.IsTrue(D.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                C
            }));
        }

        [TestCategory("IndirectSiblingDependencies")]
        [TestMethod]
        public void LongCircularReferences()
        {
            var nodes = OrderingTestFactory.CreateNodeList(@"
            A -> B
            B -> C
            C -> A
            ");

            var A = nodes.WithName("A");
            var B = nodes.WithName("B");
            var C = nodes.WithName("C");
            
            Assert.IsTrue(A.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                B,C
            }));

            Assert.IsTrue(B.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                A,C
            }));

            Assert.IsTrue(C.IndirectSiblingDependencies().SetEquals(new HashSet<Node>{
                A,B
            }));
        }
    }
}