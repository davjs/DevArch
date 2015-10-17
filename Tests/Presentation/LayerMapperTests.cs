
using System.Linq;
using Logic.Analysis.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

namespace Analysis.Tests.Presentation
{
    [TestClass]
    public class LayerMapperTests
    {
        [TestMethod]
        public void NodeViewModelToLayerViewModelTest()
        {
            var root = new Node("Root");
            { 
                var top = new Node("Top") {Horizontal = true};
                var bottom = new Node("Bottom") { Horizontal = true };

                var left = new Node("Left");
                var right = new Node("Right");


                root.AddChild(top);
                root.AddChild(bottom);

                top.AddChild(left);
                top.AddChild(right);
                bottom.AddChild(left);
                bottom.AddChild(right);
            }

            var viewModel = LayerMapper.NodeViewModelToLayerViewModel(root);

            var vtop = viewModel.Children.First();
            var vbottom = viewModel.Children.Last();
            Assert.AreEqual(2, vtop.Columns);
            Assert.AreEqual(2, vbottom.Columns);

            Assert.AreEqual(0, vtop.Row);

            Assert.AreEqual(1, vbottom.Row);
            Assert.AreEqual(1, vbottom.Row);

            Assert.AreEqual(0, vtop.Children.First().Column);
            Assert.AreEqual(1, vtop.Children.Last().Column);

            Assert.AreEqual(0, vbottom.Children.First().Column);
            Assert.AreEqual(1, vbottom.Children.Last().Column);
        }
    }
}