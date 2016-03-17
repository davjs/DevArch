using System.Collections.Generic;
using System.Linq;
using Logic.Ordering;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Ordering
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
            var groups = SiblingReorderer.FindDependencies(firstLayer, nextLayer);
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
            var dependencies = SiblingReorderer.FindDependencies(firstLayer, nextLayer).ToList();
            var groups = SiblingReorderer.FindPotentialDependencyGroups(dependencies);
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
            var groups = SiblingReorderer.FindDependencyPatterns(firstLayer, nextLayer);
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
            var dependencies = SiblingReorderer.FindDependencies(firstLayer, nextLayer).ToList();
            var groups = SiblingReorderer.FindPotentialDependencyGroups(dependencies);
            Assert.AreEqual(3, groups.Count());
            Assert.IsTrue(groups.Last().Dependants.SetEquals(new List<Node>{B,C}));
            //1 {A -> B , C} 
            //2 {A -> B}
            //3 {A -> C}
        }


        [TestMethod]
        public void FindsInnerPatternsFirst()
        {
            var nodesList = OrderingTestFactory.CreateNodeList(
            @"
            CommandBase ->
            ViewDiaCmd -> CommandBase, DevArch
            MainWindow -> DevArch
            DevArch -> DiagramDefinition
            DiagramDefinitonParser -> DiagramDefinition
            DiagramDefinition ->
            ");
            var firstLayer = SiblingReorderer.GetFacadeNodes(nodesList);
            var nextLayer = SiblingReorderer.GetFacadeNodes(new HashSet<Node>(nodesList.Except(firstLayer)));
            var pattern = SiblingReorderer.FindDependencyPatterns(firstLayer, nextLayer);
            // Nodes depending on commandbase should be merged before those dependant on devarch, because the former is a subset of the latter
            Assert.AreEqual("CommandBase", pattern.First().Dependants.First().Name);
            Assert.AreEqual(1, pattern.Count);
        }

        [TestMethod]
        public void FindsInnerPatternAmongNestedPatterns()
        {
            var nodesList = new HashSet<Node>();

            var i = 0;
            var left = new Node("Left" + i);
            nodesList.Add(left);
            var prevDown = left;

            while (++i < 10)
            {
                var nextRight = new Node("Right" + i);
                var nextDown = new Node("Down" + (i + 1));
                prevDown.SiblingDependencies.Add(nextDown);
                nextRight.SiblingDependencies.Add(nextDown);
                prevDown = nextDown;
                nodesList.Add(nextRight);
                nodesList.Add(nextDown);
            }

            var firstLayer = SiblingReorderer.GetFacadeNodes(nodesList);
            var nextLayer = SiblingReorderer.GetFacadeNodes(nodesList.SiblingDependencies());
            var pattern = SiblingReorderer.FindDependencyPatterns(firstLayer, nextLayer);
            CollectionAssert.Contains(pattern.First().Referencers.ToArray(), left);
            var newList = SiblingReorderer.LayOutSiblingNodes(nodesList);

            var toIterate = newList.Last();
            i-=1;
            while (toIterate != null)
            {
                var right = toIterate.Childs.Last();
                Assert.AreEqual("Right" + i, right.Name);

                var container = toIterate?.Childs?.First();
                if (!container.Childs.Any())
                    break;
                var down = container.Childs.First();
                toIterate = container.Childs.Last();
                Assert.AreEqual("Down" + i, down.Name);
                i--;
            }
        }


    }
}