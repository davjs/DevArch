using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

namespace Tests.Units.Presentation
{
    [TestClass]
    public class ColorRangeTests
    {
        [TestMethod]
        public void DivideTest()
        {
            var r = new ColorRange();
            Assert.AreEqual(0,r.Bottom);
            Assert.AreEqual(1, r.Top);
            var newRanges = r.Divide(2).ToList();
            var lowRange = newRanges.ElementAt(0);
            var highRange = newRanges.ElementAt(1);
            Assert.AreEqual(0,lowRange.Bottom);
            Assert.AreEqual(0.5, lowRange.Top);
            Assert.AreEqual(0.5, highRange.Bottom);
            Assert.AreEqual(1, highRange.Top);
        }
        /*[TestMethod]
        public void PaintLayerTest()
        {
            var layers = new List<LayerViewModel>();
            var A = new LayerViewModel {Name = "A"};
            var B = new LayerViewModel { Name = "B" };
            layers.Add(A);
            layers.Add(B);
            LayerMapper.PaintLayers(layers);
            Assert.AreEqual(1, A.Color.H);
            Assert.AreEqual(0.5,B.Color.H);
        }*/
    }
}