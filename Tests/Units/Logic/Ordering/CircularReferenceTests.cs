using System.Collections.Generic;
using System.Linq;
using Logic.Ordering;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Ordering
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
            
            var newList = SiblingReorderer.OrderChildsBySiblingsDependencies(new List<Node> { a , b });
            Assert.IsTrue(newList.First() is CircularDependencyHolderNode);
        }

        [TestCategory("SiblingOrder.Circular")]
        [TestMethod]
        public void ResolvesIndirectCircularReference()
        {
            var nodesList = OrderingTestFactory.CreateNodeList(
            @"
            Filtering -> SemanticTree
            Integration -> DiagramDefinitionParser
            DiagramDefinitionParser -> Filtering
            SemanticTree -> Integration"
            );

            var newList = SiblingReorderer.OrderChildsBySiblingsDependencies(nodesList.ToList());
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

            var newList = SiblingReorderer.OrderChildsBySiblingsDependencies(new List<Node> { a, b ,c });
            Assert.IsTrue(newList.First() is CircularDependencyHolderNode);
        }

        [TestCategory("SiblingOrder.Circular")]
        [TestMethod]
        public void FindCircularReferenceWhenItsLast()
        {
            var nodes = OrderingTestFactory.CreateNodeList(
            @"
            A -> B
            B -> A
            B -> C
            A -> C
            C ->
            ");
            
            CircularReferenceFinder.FindCircularReferences(nodes);
            Assert.AreEqual(typeof(CircularDependencyHolderNode), nodes.First().GetType());
        }

        //TODO: Undefined behavior
        [TestCategory("SiblingOrder.Circular")]
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
            
            var childList = new HashSet<Node> { top, left, right };
            CircularReferenceFinder.FindCircularReferences(childList);
            Assert.AreEqual(typeof(CircularDependencyHolderNode), childList.First().GetType());
            //TODO: Define wanted behaviour
        }
    }
}