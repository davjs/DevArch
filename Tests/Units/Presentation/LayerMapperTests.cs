using System.Linq;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
using Presentation.ViewModels;

namespace Tests.Units.Presentation
{
    [TestClass]
    public class LayerMapperTests
    {
        [TestMethod]
        public void NodeViewModelToLayerViewModelTest()
        {
            var root = new Node("Root");
             
            var top = new Node("Top") {Orientation = OrientationKind.Horizontal};
            var bottom = new Node("Bottom") { Orientation = OrientationKind.Horizontal };

            var left = new Node("Left");
            var right = new Node("Right");
                
            root.AddChild(top);
            root.AddChild(bottom);

            top.AddChild(left);
            top.AddChild(right);
            bottom.AddChild(left);
            bottom.AddChild(right);
            

            var viewModel = LayerMapper.NodeViewModelToLayerViewModel(root);

            var vtop = viewModel.Children.First() as LayerViewModel;
            var vbottom = viewModel.Children.Last() as LayerViewModel;
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

        [TestMethod]
        public void AddsArrows()
        {
            var rootroot = new Node("RootRoot");
            var root = new Node("Root");
            var top = new Node("Top") { Orientation = OrientationKind.Horizontal };
            var bottom = new Node("Bottom") { Orientation = OrientationKind.Horizontal };
            var left = new Node("Left");
            var right = new Node("Right");

            rootroot.AddChild(root);
            root.AddChild(top);
            root.AddChild(bottom);

            top.AddChild(left);
            top.AddChild(right);
            bottom.AddChild(left);
            bottom.AddChild(right);


            var viewModelRoot = LayerMapper.TreeModelToArchViewModel(rootroot,true);
            var viewModel = viewModelRoot.Layers.First() as LayerViewModel;
            Assert.AreEqual(3, viewModel.Children.Count());
            var vtop = viewModel.Children.First() as LayerViewModel;
            var arrow = viewModel.Children.ElementAt(1);
            var vbottom = viewModel.Children.Last() as LayerViewModel;

            Assert.AreEqual(2, vtop.Columns);
            Assert.AreEqual(2, vbottom.Columns);

            Assert.AreEqual(0, vtop.Row);
            Assert.AreEqual(1, arrow.Row);
            Assert.AreEqual(2, vbottom.Row);

            Assert.AreEqual(0, vtop.Children.First().Column);
            Assert.AreEqual(1, vtop.Children.Last().Column);

            Assert.AreEqual(0, vbottom.Children.First().Column);
            Assert.AreEqual(1, vbottom.Children.Last().Column);
        }
    }
}