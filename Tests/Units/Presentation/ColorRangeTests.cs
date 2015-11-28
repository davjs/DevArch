using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
using Presentation.Coloring.ColoringAlgorithms;
using Presentation.ViewModels;

namespace Tests.Units.Presentation
{
    [TestClass]
    public class ColorRangeTests
    {
        [TestCategory("Coloring")]
        [TestMethod]
        public void DivideTest()
        {
            var divisor = new HueRangeDivisor();
            var startColor = divisor.GetStartingColorData();
            var two = divisor.GetDistinctColors(startColor, 2);
            var c1 = two.First().GetColor().H;
            var c2 = two.Last().GetColor().H;
            Assert.AreEqual(0.75,c1);
            Assert.AreEqual(0.25, c2);
        }
        [TestCategory("Coloring")]
        [TestMethod]
        public void PaintLayerTest()
        {
            var layers = new List<LayerViewModel>();
            var A = new LayerViewModel {Name = "A"};
            var B = new LayerViewModel { Name = "B" };
            layers.Add(A);
            layers.Add(B);
            LayerMapper.PaintLayers(layers);
            Assert.AreEqual(0.75, A.Color.H);
            Assert.AreEqual(0.25,B.Color.H);
        }
    }
}