using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analysis.Tests
{
    [TestClass]
    public class SiblingOrderTests
    {
        [TestMethod]
        public void SiblingsAreOrderedByDependency()
        {
            var a = new Node("A");
            var b = new Node("B");
            var siblings = new List<Node> {a, b};
            Assert.IsTrue(siblings.SequenceEqual(new List<Node> { a, b }));
            Assert.IsFalse(siblings.SequenceEqual(new List<Node> { b, a }));
            a.SiblingDependencies.Add(b);
            siblings = Analyser.OrderChildsBySiblingsDependencies(siblings).ToList();
            Assert.IsTrue(siblings.SequenceEqual(new List<Node> { b, a }));
            Assert.IsFalse(siblings.SequenceEqual(new List<Node> { a, b }));
        }


        [TestMethod]
        public void DiscoversAnonymousLayer()
        {
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            b.SiblingDependencies.Add(a);
            c.SiblingDependencies.Add(a);

            var newSiblings = Analyser.OrderChildsBySiblingsDependencies(new List<Node> {a, b, c});

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

            var newSiblings = Analyser.OrderChildsBySiblingsDependencies(new List<Node> {a});
            var newRoot = newSiblings.First();
            Assert.IsTrue(newRoot.Horizontal);
            CollectionAssert.Contains(newRoot.Childs.ToArray(), b);
            CollectionAssert.Contains(newRoot.Childs.ToArray(), c);
        }
    }
}