using System;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Building
{
    [TestClass]
    public class SemanticTreeDebugStringTests
    {
        [TestMethod]
        public void TestSerializeSmallTree()
        {
            var top = new Node("Top");
            var left = new Node("Left");
            var right = new Node("Right");

            top.AddChild(left);
            top.AddChild(right);
            Assert.AreEqual("Top = (Left,Right)", top.ToString());
        }
    }
}
