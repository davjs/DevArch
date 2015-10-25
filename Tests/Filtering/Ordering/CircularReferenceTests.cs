using System.Collections.Generic;
using System.Linq;
using Logic.Analysis;
using Logic.Building.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Filtering.Ordering
{
    [TestClass]
    public class CircularReferenceTests
    {
        [TestCategory("SiblingOrder.Circular")]
        [TestMethod]
        public void FindCircularReference()
        {
            var a = new Node("A");
            var b = new Node("B");
            
            a.SiblingDependencies.Add(b);
            b.SiblingDependencies.Add(a);
            
            var newList = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node> { a , b });
            Assert.IsTrue(newList.First() is CircularDependencyHolderNode);
        }

        [TestCategory("SiblingOrder.Circular")]
        [TestMethod]
        public void FindCircularReferenceWhenItsFirst()
        {
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            a.SiblingDependencies.Add(b);
            b.SiblingDependencies.Add(a);
            c.SiblingDependencies.Add(b);

            var newList = SiblingReordrer.OrderChildsBySiblingsDependencies(new List<Node> { a, b ,c });
            Assert.IsTrue(newList.First() is CircularDependencyHolderNode);
        }

        [TestCategory("SiblingOrder.Circular")]
        [TestMethod]
        public void FindCircularReferenceWhenItsLast()
        {
            var a = new Node("A");
            var b = new Node("B");
            var c = new Node("C");

            a.SiblingDependencies.Add(b);
            b.SiblingDependencies.Add(a);
            b.SiblingDependencies.Add(c);
            a.SiblingDependencies.Add(c);

            var childList = new List<Node> { a, b, c };
            SiblingReordrer.FindCircularReferences(ref childList);
            Assert.AreEqual(typeof(CircularDependencyHolderNode), childList.Last().GetType());
        }

        //TODO: Undefined behavior
        /*[TestCategory("SiblingOrder.Circular")]
        [TestMethod]
        public void SolvesCircularReferenceInvolvingMoreThreeComponents()
        {
            var top = new Node("top");
            var left = new Node("left");
            var right = new Node("right");
            
            //Left and right depend on eachother
            left.SiblingDependencies.Add(right);
            right.SiblingDependencies.Add(left);
            //Left depends on top, top depends on right
            left.SiblingDependencies.Add(top);
            top.SiblingDependencies.Add(right);
            
            var childList = new List<Node> { top, left, right };
            SiblingReordrer.FindCircularReferences(ref childList);
            Assert.AreEqual(typeof(CircularDependencyHolderNode), childList.First().GetType());
            //TODO: Define wanted behaviour
        }*/
    }
}