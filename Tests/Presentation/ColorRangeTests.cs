using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

namespace Tests.Presentation
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
    }
}