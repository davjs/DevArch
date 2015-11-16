using System.Collections.Generic;
using System.Linq;
using Logic.Filtering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic
{
    [TestClass]
    public class PatternFinderTests
    {
        [TestMethod]
        public void FindPatternTest()
        {
            Assert.AreEqual("Service",PatternFinder.FindNamingPatterns(new List<string>() {"CalculatorService","ParserService"}).First());
        }
    }
}