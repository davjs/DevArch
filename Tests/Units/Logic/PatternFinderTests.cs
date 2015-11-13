using System.Collections.Generic;
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
            Assert.AreEqual("Service",PatternFinder.FindNamingPattern(new List<string> {"CalculatorService","ParserService"}));
        }
    }
}