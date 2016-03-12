using System;
using System.Linq;
using FluentAssertions;
using Logic.Common;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic
{
    [TestClass]
    public class CloneTests
    {
        [TestMethod]
        public void DeepCloningCopiesType()
        {
            new ClassNode(null,null,1)
                .DeepClone()
                .Should()
                .BeOfType<ClassNode>();
        }

        [TestMethod]
        public void ClonesChildren()
        {
            var child = new Node("c1");
            var parent = new ClassNode(null, null, 1);
            parent.AddChild(child);
            var parent2 = parent.DeepClone();
            
            //Assert
            parent2.Childs.Should()
                .NotBeEmpty()
                .And.NotContain(child);
            parent2.Childs.First().Name.Should().Be("c1");
        }

        [TestMethod]
        public void ModifyingClonedTreeDoesNotModifyOriginal()
        {
            var originalChild1 = new Node("c1");
            var parent = new ClassNode(null, null, 1);
            parent.AddChild(originalChild1);

            var clonedParent = parent.DeepClone();
            var clonedParent2 = parent.DeepClone();
            clonedParent.FilterAllChilds();
            
            // Assert:
            clonedParent.Childs.Should().BeEmpty();
            clonedParent2.Childs.Should().NotBeEmpty();
        }

        [TestMethod]
        public void ClonesDependencies()
        {
            var originalChild1 = new Node("c1");
            var originalChild2 = new Node("c2");
            var parent = new ClassNode(null, null, 1);
            parent.AddChild(originalChild1);
            parent.AddChild(originalChild2);
            originalChild1.Dependencies.Add(originalChild2);

            var parent2 = parent.DeepClone();

            var clonedChild1 = parent2.Childs.WithName("c1");
            var clonedChild2 = parent2.Childs.WithName("c2");
            //Assert
            clonedChild1.Dependencies.Should()
                .Contain(clonedChild2)
                .And
                .NotContain(originalChild2);
        }
    }
}
