using System.Collections.Generic;
using System.Linq;
using Analysis.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analysis.Tests
{
    [TestClass]
    public class SiblingOrderTests
    {
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


        [TestMethod]
        public void DiscoversAnonymousLayer()
        {
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            b.SiblingDependencies.Add(a);
            c.SiblingDependencies.Add(a);

            var newSiblings = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node> {a, b, c});

            var anonymousLayer = newSiblings.OfType<SiblingHolderNode>().FirstOrDefault();
            Assert.IsNotNull(anonymousLayer);
            CollectionAssert.Contains(anonymousLayer.Childs.ToArray(),b);
            CollectionAssert.Contains(anonymousLayer.Childs.ToArray(), c);
        }

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
            SiblingReordrer.RegroupSiblingNodes(a,new List<Node> { c,b,a},ref newList);
            Assert.IsTrue(newList.SequenceEqual(new List<Node> {a,b,c}));
        }
    }
}