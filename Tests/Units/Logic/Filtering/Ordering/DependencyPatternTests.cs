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
            // {C -> A}
            // {C -> X}
            // {D -> X}
            // {D -> B}
            // {E -> B}
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
            var groups = SiblingReordrer.FindPotentialDependencyGroups(firstLayer, nextLayer);
            Assert.AreEqual(9, groups.Count());
            Assert.IsTrue(groups.Any(x => x.Referencers.SetEquals(new []{D , E})));
            //1 {C -> A , X} 
            //2 {C -> A}     SELECT
            //3 {C -> X}
            //4 {D -> X , B}
            //5 {D -> X}
            //6 {C , D -> X}
            //7 {D -> B}
            //8 {E -> B}
            //9 {D, E -> B}  Most referencers, Remove all posibilities of X
        }
        [TestMethod]
        public void FindDependencyGroupsTest3()
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
            Assert.AreEqual(2, groups.Count());
            //1 {C -> A , X} Cant choose, would hide D -> X dependency
            //2 {C -> A}
            //3 {C -> X}
            //4 {D -> X , B} Cant choose, would hide E -> B dependency
            //5 {D -> X}
            //6 {C , D -> X} Most referencers, Remove all posibilities of B
            //7 {D -> B}
            //8 {E -> B}
            //9 {D, E -> B}  Most referencers, Remove all posibilities of X
        }

        [TestMethod]
        public void FindDependencyGroupsTest2()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));

            A.SiblingDependencies.Add(B);
            A.SiblingDependencies.Add(C);
            
            //0.  B   C
            //1.    A
            var firstLayer = new List<Node> { A, B, C };
            var nextLayer = new List<Node> { A, B, C };
            var groups = SiblingReordrer.FindPotentialDependencyGroups(firstLayer, nextLayer);
            Assert.AreEqual(3, groups.Count());
            Assert.IsTrue(groups.Last().Dependants.SetEquals(new List<Node>{B,C}));
            //1 {A -> B , C} 
            //2 {A -> B}
            //3 {A -> C}
        }





    }
}