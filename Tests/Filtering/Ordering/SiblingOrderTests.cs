using System.Collections.Generic;
using System.Linq;
using Logic.Analysis;
using Logic.Analysis.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analysis.Tests.Filtering.Ordering
{

    [TestClass]
    public class SiblingOrderTests
    {
        [TestCategory("SiblingOrder")]
        [TestMethod]
        public void TwoSiblingsAreOrderedByDependency()
        {
            var a = new Node("A");
            var b = new Node("B");
            var siblings = new List<Node> {a, b};
            Assert.IsTrue(siblings.SequenceEqual(new List<Node> { a, b }));
            Assert.IsFalse(siblings.SequenceEqual(new List<Node> { b, a }));
            a.SiblingDependencies.Add(b);
            siblings = SiblingReordrer.OrderChildsBySiblingsDependencies(siblings).ToList();
            Assert.IsTrue(siblings.SequenceEqual(new List<Node> { b, a }));
            Assert.IsFalse(siblings.SequenceEqual(new List<Node> { a, b }));
        }

        [TestCategory("SiblingOrder")]
        [TestMethod]
        public void ThreeSiblingsAreOrderedByDependency()
        {
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");
            var siblings = new List<Node> { a, b,c };
            Assert.IsTrue(siblings.SequenceEqual(new List<Node> { a, b ,c }));
            Assert.IsFalse(siblings.SequenceEqual(new List<Node> { c, b, a }));
            a.SiblingDependencies.Add(b);
            b.SiblingDependencies.Add(c);
            siblings = SiblingReordrer.OrderChildsBySiblingsDependencies(siblings).ToList();
            Assert.IsTrue(siblings.SequenceEqual(new List<Node> {c, b, a }));
            Assert.IsFalse(siblings.SequenceEqual(new List<Node> { a, b ,c}));
        }


        [TestCategory("SiblingOrder")]
        [TestMethod]
        public void DiscoversAnonymousLayer()
        {
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            b.SiblingDependencies.Add(a);
            c.SiblingDependencies.Add(a);

            var newSiblings = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node> {a, b, c});

            var anonymousLayer = newSiblings.OfType<SiblingHolderNode>().LastOrDefault();
            Assert.IsNotNull(anonymousLayer);
            CollectionAssert.Contains(anonymousLayer.Childs.ToArray(),b);
            CollectionAssert.Contains(anonymousLayer.Childs.ToArray(), c);
        }

        /*[TestCategory("SiblingOrder")]
        [TestMethod]
        public void DiscoversAnonymousVerticalLayers()
        {
            var a = new Node("A");
            var b = new Node("B");
            
            var one = new Node("1");
            var two = new Node("2");

            b.SiblingDependencies.Add(a);
            
            two.SiblingDependencies.Add(one);
            
            var newSiblings = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node> { a, b,one,two});

            var anonymousHorizontalLayer = newSiblings.FirstOrDefault();
            Assert.IsNotNull(anonymousHorizontalLayer);
            Assert.AreEqual(typeof(SiblingHolderNode),anonymousHorizontalLayer.GetType());
            Assert.IsTrue(anonymousHorizontalLayer.Childs.First() is VerticalSiblingHolderNode);
            Assert.IsTrue(anonymousHorizontalLayer.Childs.Last() is VerticalSiblingHolderNode);
        }*/


        [TestCategory("SiblingOrder")]
        [TestMethod]
        public void PutsIndependentNodesOnTop()
        {
            var a = new Node("A");
            var b = new Node("B");
            
            var one = new Node("1");
            var two = new Node("2");


            one.SiblingDependencies.Add(a);
            two.SiblingDependencies.Add(b);

            var newSiblings = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node> { a, b, one, two });

            var anonymousHorizontalLayer = newSiblings.FirstOrDefault();
            Assert.IsNotNull(anonymousHorizontalLayer);
            Assert.AreEqual(typeof(SiblingHolderNode), anonymousHorizontalLayer.GetType());
            Assert.AreEqual(anonymousHorizontalLayer.Childs.First(),a);
            Assert.AreEqual(anonymousHorizontalLayer.Childs.Last(), b);
        }

        [TestCategory("SiblingOrder")]
        [TestMethod]
        public void PutsIndependentSiblingsIntoHorizontalLayer()
        {
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            a.AddChild(b);
            a.AddChild(c);

            var newSiblings = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node> {a});
            var newRoot = newSiblings.First();
            Assert.IsTrue(newRoot.Horizontal);
            CollectionAssert.Contains(newRoot.Childs.ToArray(), b);
            CollectionAssert.Contains(newRoot.Childs.ToArray(), c);
        }

        [TestCategory("SiblingOrder")]
        [TestMethod]
        public void FindsDirectionBetweenMultiLayers()
        {
            var a = new Node("A");//-
            var b = new Node("B");//^
            var c = new Node("C");//^^

            c.SiblingDependencies.Add(b);
            c.SiblingDependencies.Add(a);
            b.SiblingDependencies.Add(a);

            var newList = new List<Node>();
            SiblingReordrer.RegroupSiblingNodes(new List<Node>(), new List<Node> { c,b,a},ref newList);
            Assert.IsTrue(newList.SequenceEqual(new List<Node> {a,b,c}));
        }
    }
}