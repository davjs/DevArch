using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnvDTE;
using Logic.Analysis.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
using Presentation.ViewModels;
using Thread = System.Threading.Thread;

namespace Analysis.Tests.Presentation
{
    [TestClass]
    public class LayerMapperTests
    {
        private readonly PngGenerationTests _pngGenerationTests = new PngGenerationTests();

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