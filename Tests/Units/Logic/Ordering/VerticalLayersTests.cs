using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Logic;
using Logic.Ordering;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Ordering
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

            var newList = SiblingReorderer.OrderChildsBySiblingsDependencies(new List<Node>
            {
                A,B,C,D
            }).ToList();

            // A   
            // B   C
            //   D

            newList.Count.Should().Be(2);
            newList.Last().Should().Be(D);
            var hor = newList.First();
            hor.Childs.Last().Should().Be(C);
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
            D.SiblingDependencies.Add(X);
            B.SiblingDependencies.Add(A);
            A.SiblingDependencies.Add(X);
            C.SiblingDependencies.Add(X);
            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                A,
                B,
                C,
                D,
                X
            });
            //   X
            // A   
            // B   C
            //   D

            newChildOrder.Count.Should().Be(3);
            newChildOrder.Last().Should().Be(D);
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

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                A,
                B,
                C,
                D,
                E
            });

            //0. A B  
            //0.  C   D
            //1.    E

            newChildOrder.Count.Should().Be(2);
            var hor = newChildOrder.First();
            Assert.IsTrue(hor is SiblingHolderNode);
            var left = hor.Childs.First();
            var right = hor.Childs.Last();
            Assert.AreEqual(D, right);
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            hor.Childs.Count.Should().Be(2);
            Assert.AreEqual(E, newChildOrder.Last());
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsTwoLongSeparateVerticalLayers()
        {
            Node A = new Node(nameof(A));
            Node B = new Node(nameof(B));
            Node C = new Node(nameof(C));
            Node D = new Node(nameof(D));
            Node E = new Node(nameof(E));
            Node F = new Node(nameof(F));
            Node G = new Node(nameof(G));
            Node X = new Node(nameof(X));

            C.SiblingDependencies.Add(A);
            E.SiblingDependencies.Add(C);

            D.SiblingDependencies.Add(B);
            F.SiblingDependencies.Add(D);

            G.SiblingDependencies.Add(E);
            G.SiblingDependencies.Add(F);

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                A, B, C, D, E, F, G
            });

            //0.  A     B
            //0.  C     D 
            //0.  E     F
            //1.     G

            newChildOrder.Count.Should().Be(2);
            var hor = newChildOrder.First();
            Assert.IsTrue(hor is HorizontalSiblingHolderNode);
            var left = hor.Childs.First();
            left.Childs.Count.Should().Be(3);

            var ACE = new List<Node> {A, C, E};
            var BDF = new List<Node> {B, D, F};
            Assert.IsTrue(left.Childs.SequenceEqual(ACE) || left.Childs.SequenceEqual(BDF));
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void FindsCommonDependorAfterTwoLongVerticalLayers()
        {
            var A = new Node("A");
            var B = new Node("B");
            var C = new Node("C");
            var D = new Node("D");
            var E = new Node("E");
            var F = new Node("F");
            var X = new Node("X");

            A.SiblingDependencies.Add(X);
            B.SiblingDependencies.Add(X);

            C.SiblingDependencies.Add(A);
            E.SiblingDependencies.Add(C);

            D.SiblingDependencies.Add(B);
            F.SiblingDependencies.Add(D);


            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                A, B, C, D, E, F, X
            });

            //0.     X
            //0.  A     B 
            //0.  C     D 
            //0.  E     F 

            newChildOrder.Count.Should().Be(2);
            var hor = newChildOrder.Last();
            Assert.IsTrue(hor is SiblingHolderNode && !(hor is VerticalSiblingHolderNode));
            hor.Childs.Count.Should().Be(2);
            var left = hor.Childs.First();
            var right = hor.Childs.Last();
            left.Should().BeOfType<VerticalSiblingHolderNode>();
            right.Should().BeOfType<VerticalSiblingHolderNode>();
            left.Childs.Count.Should().Be(3);
            right.Childs.Count.Should().Be(3);
            left.Childs.Should().NotContain(X);
            right.Childs.Should().NotContain(X);
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

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                A, B, C, D, E
            });

            //0.  A    B X
            //0.  C     D 
            //1.     E

            newChildOrder.Count.Should().Be(2);
            var hor = newChildOrder.First();
            Assert.IsTrue(hor is SiblingHolderNode && !(hor is VerticalSiblingHolderNode));
            hor.Childs.Count.Should().Be(2);
            var left = hor.Childs.First();
            var right = hor.Childs.Last();
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            left.Childs.First().Should().Be(A);
            left.Childs.Last().Should().Be(C);
            var topRight = right.Childs.First();
            topRight.Should().BeOfType<HorizontalSiblingHolderNode>();
            right.Childs.Last().Should().Be(D);
            newChildOrder.Last().Should().Be(E);
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

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                A, B, C, D, E, F
            });

            //0.  A      B 
            //0.  C     D-E
            //1.     F

            newChildOrder.Count.Should().Be(2);
            //0
            var hor = newChildOrder.First();
            Assert.AreEqual(OrientationKind.Horizontal, hor.Orientation);
            hor.Childs.Count.Should().Be(2);
            var right = hor.Childs.First();
            var left = hor.Childs.Last();
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            Assert.AreEqual(A, left.Childs.First());
            Assert.AreEqual(C, left.Childs.Last());
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            right.Childs.First().Should().Be(B);
            var rightBot = right.Childs.Last();
            rightBot.Orientation.Should().Be(OrientationKind.Horizontal);
            CollectionAssert.Contains(rightBot.Childs.ToArray(), D);
            CollectionAssert.Contains(rightBot.Childs.ToArray(), E);
            //1
            newChildOrder.Last().Should().Be(F);
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

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                A, B, C, D, E, F, G
            });

            //0.  A     B C 
            //0.  D     F-E
            //1.     G

            newChildOrder.Count.Should().Be(2);
            //0
            var hor = newChildOrder.First();
            Assert.IsTrue(hor is HorizontalSiblingHolderNode);
            hor.Childs.Count.Should().Be(2);
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
            rightTop.Childs.Count.Should().Be(2);
            rightBot.Childs.Count.Should().Be(2);
            CollectionAssert.Contains(rightBot.Childs.ToArray(),E);
            CollectionAssert.Contains(rightBot.Childs.ToArray(), F);
            //1
            Assert.AreEqual(G, newChildOrder.Last());
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void DoesNotProduceDuplicateNodes()
        {
            var Node = new Node("Node");
            var Tree = new Node("Tree");

            var SiblingHolderNode = new Node("SiblingHolderNode");
            var CircularDependencyHolderNode = new Node("CircularDependencyHolderNode");
            var NodeExtensions = new Node("NodeExtensions");
            var ClassNode = new Node("ClassNode");
            var ProjectNode = new Node("ProjectNode");

            Node.SiblingDependencies.Add(Tree);
            SiblingHolderNode.SiblingDependencies.Add(Node);
            CircularDependencyHolderNode.SiblingDependencies.Add(SiblingHolderNode);
            CircularDependencyHolderNode.SiblingDependencies.Add(Node);
            NodeExtensions.SiblingDependencies.Add(Node);
            NodeExtensions.SiblingDependencies.Add(Tree);

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                Node,SiblingHolderNode,CircularDependencyHolderNode,NodeExtensions,
                Tree,ClassNode,ProjectNode
            });
            //         Node
            //      Sib   NodeExt
            //      Circ

            (newChildOrder.Count(x => x.Name == "Node") +
                newChildOrder.Count(x => x.DescendantNodes().WithName("Node") != null)).Should().Be(1);
        }


        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void DoesNotProduceDuplicateRootScopes()
        {
            var diagramDefinition = new Node("DiagramDefinition");
            var diagramDefinitionParser = new Node("DiagramDefinitionParser");
            var rootScope = new Node("RootScope");
            var filters = new Node("Filters");
            var modelFilterer = new Node("ModelFilterer");
            
            diagramDefinition.SiblingDependencies.Add(rootScope);
            diagramDefinitionParser.SiblingDependencies.Add(rootScope);
            diagramDefinitionParser.SiblingDependencies.Add(diagramDefinition);
            modelFilterer.SiblingDependencies.Add(filters);

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                diagramDefinition,diagramDefinitionParser,rootScope,filters,modelFilterer
            });


            //     RootScope
            //    DiagramDefinition        Filters
            //  DiagramDefinitionParser ModelFilterer

            (newChildOrder.Count(x => x.Name == "RootScope") +
                newChildOrder.Count(x => x.DescendantNodes().WithName("RootScope") != null)).Should().Be(1);
        }


        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void PutsPartlyCommonDependencyOnNextLayer()
        {
            var nodesList = OrderingTestFactory.CreateNodeList(
                @"A ->
                B ->
                C -> A,X
                D -> B,X
                E -> B
                F -> C,D,E
                X ->");

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(nodesList);

            //             
            //1.  A  X   B
            //1.  C     D E
            //2.     F

            newChildOrder.Count.Should().Be(3);
            //0
            Assert.AreEqual("X", newChildOrder.First().Name);
            var hor = newChildOrder.ElementAt(1);
            Assert.AreEqual(OrientationKind.Horizontal, hor.Orientation);
            hor.Childs.Count.Should().Be(2);
            var left = hor.Childs.First();
            var right = hor.Childs.Last();
            Assert.IsTrue(left is VerticalSiblingHolderNode);
            left.Childs.First().Name.Should().Be("A");
            Assert.AreEqual("C", left.Childs.Last().Name);
            Assert.IsTrue(right is VerticalSiblingHolderNode);
            Assert.AreEqual("B", right.Childs.First().Name);
            var rightBot = right.Childs.Last();
            Assert.AreEqual(OrientationKind.Horizontal, rightBot.Orientation);
            Assert.IsTrue(rightBot.Childs.Any(x => x.Name == "D"));
            Assert.IsTrue(rightBot.Childs.Any(x => x.Name == "E"));
            //1
            Assert.AreEqual("F", newChildOrder.Last().Name);
        }

        [TestCategory("SiblingOrder.VerticalLayers")]
        [TestMethod]
        public void MultipleHorizontalLayers()
        {

            var nodesList = OrderingTestFactory.CreateNodeList(
                @"ColorDataWithDepth ->
                LayerViewModel ->
                RootScope ->
                ArchView ->
                Hsl -> ColorDataWithDepth
                ColorRange -> ColorDataWithDepth
                HueRangeDivisor -> ColorRange
                BitmapRenderer -> LayerMapper
                LayerMapper -> HueRangeDivisor, LayerViewModel
                DevArch -> BitmapRenderer, ArchView, LayerMapper"
                );

            var newChildOrder = SiblingReorderer.LayOutSiblingNodes(nodesList);
            
            var assertLayout =
                @"
                [
                     [                        
                        DevArch,
                        [ArchView,BitmapRenderer]
                        LayerMapper  
                        [
                            LayerViewModel,
                            [HueRangeDivisor, ColorRange]
                        ]
                    ]
                    Hsl
                ]
                ColorDataWithDepth
            ".BuildTree();

            var tree = new Node("tree");
            tree.SetChildren(newChildOrder);
            DiagramGenerator.ReverseChildren(tree);
            TestExtesions.TreeAssert.DoesNotContainDuplicates(tree);
            OrderingTestFactory.AssertLayout(assertLayout,tree);
        }
    }
}