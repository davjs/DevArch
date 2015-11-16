using System.Collections.Generic;
using System.Linq;
using Logic.Building;
using Logic.Building.SemanticTree;
using Logic.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Logic.Building.SemanticTree.OrientationKind;

namespace Tests.Units.Logic.Filtering.Ordering
{

    [TestClass]
    public class SiblingOrderTests
    {
        private readonly VerticalLayersTests _verticalLayersTests = new VerticalLayersTests();

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
            Assert.AreEqual(Horizontal,newRoot.Orientation);
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

            var newList = SiblingReordrer.RegroupSiblingNodes(new List<Node> { c,b,a});
            Assert.IsTrue(newList.SequenceEqual(new List<Node> {a,b,c}));
        }

        [TestCategory("SiblingOrder")]
        [TestMethod]
        public void Scenario()
        {
            Node semanticTreeBuilder = new Node(nameof(semanticTreeBuilder));
            Node classTreeBuilder = new Node(nameof(classTreeBuilder));
            Node projectTreeBuilder = new Node(nameof(projectTreeBuilder));
            Node semanticModelWalker = new Node(nameof(semanticModelWalker));

            classTreeBuilder.SiblingDependencies.Add(semanticModelWalker);
            semanticTreeBuilder.SiblingDependencies.Add(projectTreeBuilder);
            semanticTreeBuilder.SiblingDependencies.Add(classTreeBuilder);

            var newList = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node>
            {
                semanticTreeBuilder,classTreeBuilder,projectTreeBuilder,semanticModelWalker
            });
            Assert.AreEqual(newList.Count(),2);
        }
    }
}