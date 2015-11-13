using System.Collections.Generic;
using System.Linq;
using Logic.Building.SemanticTree;
using Logic.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Filtering.Ordering
{
    [TestClass]
    public class DependencyPatternTests
    {
        [TestMethod]
        public void FindsDependeniesTest()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));
            Node X = new Node(nameof(X));

            C.SiblingDependencies.Add(A);
            C.SiblingDependencies.Add(X);
            D.SiblingDependencies.Add(X);
            D.SiblingDependencies.Add(B);
            E.SiblingDependencies.Add(B);

            //       0      
            //1.  A  X   B
            //1.  C     D E
            var firstLayer = new List<Node> {C,D,E};
            var nextLayer = new List<Node> { A,X,B };
            var groups = SiblingReordrer.FindDependencies(firstLayer, nextLayer);
            Assert.AreEqual(5,groups.Count());
        }

        [TestMethod]
        public void FindDependencyGroupsTest()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));
            Node X = new Node(nameof(X));

            C.SiblingDependencies.Add(A);
            C.SiblingDependencies.Add(X);
            D.SiblingDependencies.Add(X);
            D.SiblingDependencies.Add(B);
            E.SiblingDependencies.Add(B);

            //       0      
            //1.  A  X   B
            //1.  C     D E
            var firstLayer = new List<Node> { C, D, E };
            var nextLayer = new List<Node> { A, X, B };
            var groups = SiblingReordrer.FindDependencyGroups(firstLayer, nextLayer);
            Assert.AreEqual(5, groups.Count());
        }

    }
}