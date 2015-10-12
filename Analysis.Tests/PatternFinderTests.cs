using Microsoft.VisualStudio.TestTools.UnitTesting;
using Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Analysis;
using Logic.Filtering;

namespace Analysis.Tests
{
    [TestClass()]
    public class PatternFinderTests
    {
        [TestMethod()]
        public void FindPatternTest()
        {
            Assert.AreEqual("Service",PatternFinder.FindNamingPattern(new List<string>() {"CalculatorService","ParserService"}));
        }
    }
}