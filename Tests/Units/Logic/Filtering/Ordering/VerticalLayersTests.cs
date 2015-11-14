using System.Collections.Generic;
using System.Linq;
using Logic.Building.SemanticTree;
using Logic.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Filtering.Ordering
{
    [TestClass]
    public class VerticalLayersTests
    {
        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsVerticalLayers()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));

            D.SiblingDependencies.Add(B);
            D.SiblingDependencies.Add(C);
            B.SiblingDependencies.Add(A);

            var newList = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node>
            {
                A,B,C,D
            }).ToList();

            // A   
            // B   C
            //   D

            Assert.AreEqual(newList.Count, 2);
            Assert.AreEqual(newList.Last(), D);
            var hor = newList.First();
            Assert.AreEqual(C,hor.Childs.First());
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsVerticalLayers2()
        {
            Node X = new Node(nameof(X));
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));

            D.SiblingDependencies.Add(B);
            D.SiblingDependencies.Add(C);
            B.SiblingDependencies.Add(A);
            A.SiblingDependencies.Add(X);
            C.SiblingDependencies.Add(X);
            var newChildOrder = new List<Node>();
            SiblingReordrer.RegroupSiblingNodes(new List<Node>
            {
                A,
                B,
                C,
                D,
                X
            },ref newChildOrder);
            //   X
            // A   
            // B   C
            //   D

            Assert.AreEqual(newChildOrder.Count, 3);
            Assert.AreEqual(newChildOrder.Last(), D);
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsVerticalLayers3()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));

            E.SiblingDependencies.Add(C);
            E.SiblingDependencies.Add(D);
            C.SiblingDependencies.Add(A);
            C.SiblingDependencies.Add(B);

            var newChildOrder = new List<Node>();
            SiblingReordrer.RegroupSiblingNodes(new List<Node>
            {
                A,
                B,
                C,
                D,
                E
            }, ref newChildOrder);

            //0.     A B  
            //0.  D   C
            //1.    E

            Assert.AreEqual(2, newChildOrder.Count);
            var hor = newChildOrder.First();
            Assert.IsTrue(hor is SiblingHolderNode);
            var left = hor.Childs.First();
            var right = hor.Childs.Last();
            Assert.AreEqual(D, left);
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            Assert.AreEqual(2,hor.Childs.Count);
            Assert.AreEqual(E,newChildOrder.Last());
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsTwoSeparateVerticalLayers()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));
            Node X = new Node(nameof(X));

            E.SiblingDependencies.Add(C);
            E.SiblingDependencies.Add(D);
            C.SiblingDependencies.Add(A);
            D.SiblingDependencies.Add(B);
            D.SiblingDependencies.Add(X);

            var newChildOrder = new List<Node>();
            SiblingReordrer.RegroupSiblingNodes(new List<Node>
            {
                A,
                B,
                C,
                D,
                E
            }, ref newChildOrder);

            //0.  A    B X
            //0.  C     D 
            //1.     E

            Assert.AreEqual(2, newChildOrder.Count);
            var hor = newChildOrder.First();
            Assert.IsTrue(hor is SiblingHolderNode && !(hor is VerticalSiblingHolderNode));
            Assert.AreEqual(2, hor.Childs.Count);
            var left = hor.Childs.First();
            var right = hor.Childs.Last();
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            Assert.AreEqual(A,left.Childs.First());
            Assert.AreEqual(C, left.Childs.Last());
            var topRight = right.Childs.First();
            Assert.IsTrue(topRight is SiblingHolderNode);
            Assert.AreEqual(D, right.Childs.Last());
            Assert.AreEqual(E, newChildOrder.Last());
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsCommonUniqueDependency()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));
            Node F = new Node(nameof(F));

            F.SiblingDependencies.Add(C);
            F.SiblingDependencies.Add(D);
            F.SiblingDependencies.Add(E);
            C.SiblingDependencies.Add(A);
            D.SiblingDependencies.Add(B);
            E.SiblingDependencies.Add(B);

            var newChildOrder = new List<Node>();
            SiblingReordrer.RegroupSiblingNodes(new List<Node>
            {
                A,
                B,
                C,
                D,
                E,
                F
            }, ref newChildOrder);

            //0.  A      B 
            //0.  C     D-E
            //1.     F

            Assert.AreEqual(2, newChildOrder.Count);
            //0
            var hor = newChildOrder.First();
            Assert.AreEqual(OrientationKind.Horizontal, hor.Orientation);
            Assert.AreEqual(2, hor.Childs.Count);
            var right = hor.Childs.First();
            var left = hor.Childs.Last();
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            Assert.AreEqual(A, left.Childs.First());
            Assert.AreEqual(C, left.Childs.Last());
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            Assert.AreEqual(B, right.Childs.First());
            var rightBot = right.Childs.Last();
            Assert.AreEqual(OrientationKind.Horizontal, rightBot.Orientation);
            CollectionAssert.Contains(rightBot.Childs.ToArray(), D);
            CollectionAssert.Contains(rightBot.Childs.ToArray(), E);
            //1
            Assert.AreEqual(F, newChildOrder.Last());
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsMultipleCommonUniqueDependency()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));
            Node F = new Node(nameof(F));
            Node G = new Node(nameof(G));

            G.SiblingDependencies.Add(D);
            G.SiblingDependencies.Add(E);
            G.SiblingDependencies.Add(F);
            D.SiblingDependencies.Add(A);

            E.SiblingDependencies.Add(B);
            E.SiblingDependencies.Add(C);
            F.SiblingDependencies.Add(B);
            F.SiblingDependencies.Add(C);

            var newChildOrder = new List<Node>();
            SiblingReordrer.RegroupSiblingNodes(new List<Node>
            {
                A,
                B,
                C,
                D,
                E,
                F,
                G
            }, ref newChildOrder);

            //0.  A     B C 
            //0.  D     F-E
            //1.     G

            Assert.AreEqual(2, newChildOrder.Count);
            //0
            var hor = newChildOrder.First();
            Assert.IsTrue(hor is HorizontalSiblingHolderNode);
            Assert.AreEqual(2, hor.Childs.Count);
            var left = hor.Childs.First();
            var right = hor.Childs.Last();
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            Assert.AreEqual(A, left.Childs.First());
            Assert.AreEqual(D, left.Childs.Last());
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            var rightTop = right.Childs.First();
            var rightBot = right.Childs.Last();
            Assert.IsTrue(rightTop is HorizontalSiblingHolderNode);
            Assert.IsTrue(rightBot is HorizontalSiblingHolderNode);
            Assert.AreEqual(2, rightTop.Childs.Count);
            Assert.AreEqual(2, rightBot.Childs.Count);
            Assert.AreEqual(F, rightBot.Childs.First());
            Assert.AreEqual(E, rightBot.Childs.Last());
            //1
            Assert.AreEqual(G, newChildOrder.Last());
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void PutsPartlyCommonDependencyOnNextLayer()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));
            Node F = new Node(nameof(F));
            Node X = new Node(nameof(X));

            F.SiblingDependencies.Add(C);
            F.SiblingDependencies.Add(D);
            F.SiblingDependencies.Add(E);
            C.SiblingDependencies.Add(A);
            C.SiblingDependencies.Add(X);
            D.SiblingDependencies.Add(X);
            D.SiblingDependencies.Add(B);
            E.SiblingDependencies.Add(B);

            var newChildOrder = new List<Node>();
            SiblingReordrer.RegroupSiblingNodes(new List<Node>
            {
                A,
                B,
                C,
                D,
                E,
                F
            }, ref newChildOrder);

            //             
            //1.  A  X   B
            //1.  C     D E
            //2.     F

            Assert.AreEqual(3, newChildOrder.Count);
            //0
            Assert.AreEqual(X, newChildOrder.First());
            var hor = newChildOrder.ElementAt(1);
            Assert.AreEqual(OrientationKind.Horizontal, hor.Orientation);
            Assert.AreEqual(2, hor.Childs.Count);
            var right = hor.Childs.First();
            var left = hor.Childs.Last();
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            Assert.AreEqual(A, left.Childs.First());
            Assert.AreEqual(C, left.Childs.Last());
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            Assert.AreEqual(B, right.Childs.First());
            var rightBot = right.Childs.Last();
            Assert.AreEqual(OrientationKind.Horizontal, rightBot.Orientation);
            CollectionAssert.Contains(rightBot.Childs.ToArray(), D);
            CollectionAssert.Contains(rightBot.Childs.ToArray(), E);
            //1
            Assert.AreEqual(F, newChildOrder.Last());
        }
    }
}