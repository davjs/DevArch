using System.Collections.Generic;
using Logic.Filtering;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Ordering
{
    [TestClass]
    public class SiblindDependencyTests
    {
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