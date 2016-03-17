using System.Linq;
using FluentAssertions;
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
            

            var viewModel = LayerMapper.NodeViewModelToLayerViewModel(root,true);

            var vtop = viewModel.Children.First() as LayerViewModel;
            var vbottom = viewModel.Children.Last() as LayerViewModel;
            vtop.Columns.Should().Be(2);
            vbottom.Columns.Should().Be(2);

            vtop.Row.Should().Be(0);

            vbottom.Row.Should().Be(2);
            vbottom.Row.Should().Be(2);

            vtop.Children.First().Column.Should().Be(0);
            vtop.Children.Last().Column.Should().Be(1);

            vbottom.Children.First().Column.Should().Be(0);
            vbottom.Children.Last().Column.Should().Be(1);
        }

        [TestMethod]
        public void AddsArrows()
        {
            var rootroot = new Node("RootRoot");
            var root = new Node("Root");
            var top = new Node("Top") {Orientation = OrientationKind.Horizontal};
            var bottom = new Node("Bottom") {Orientation = OrientationKind.Horizontal};
            var left = new Node("Left");
            var right = new Node("Right");

            rootroot.AddChild(root);
            root.AddChild(top);
            root.AddChild(bottom);

            top.AddChild(left);
            top.AddChild(right);
            bottom.AddChild(left);
            bottom.AddChild(right);


            var viewModelRoot = LayerMapper.TreeModelToArchViewModel(rootroot, true, true);
            var viewModel = viewModelRoot.Layers.First() as LayerViewModel;
            viewModel.Children.Count().Should().Be(3);
            var vtop = viewModel.Children.First() as LayerViewModel;
            var arrow = viewModel.Children.ElementAt(1);
            var vbottom = viewModel.Children.Last() as LayerViewModel;

            vtop.Columns.Should().Be(2);
            vbottom.Columns.Should().Be(2);

            vtop.Row.Should().Be(0);
            arrow.Row.Should().Be(1);
            vbottom.Row.Should().Be(2);

            vtop.Children.First().Column.Should().Be(0);
            vtop.Children.Last().Column.Should().Be(1);

            vbottom.Children.First().Column.Should().Be(0);
            vbottom.Children.Last().Column.Should().Be(1);
        }
    }
}