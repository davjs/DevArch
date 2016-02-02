using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic
{
    [TestClass]
    public class DiagramDefinitionParserTests
    {
        [TestMethod]
        public void ParsesHideAnonymousLayer()
        {
            const string content = 
            @"<?xml version=""1.0"" encoding =""utf -8"" ?>
            <Diagram HideAnonymousLayers = ""false"">
              <Scope>
                <Root/>
              </Scope>
              <Output Path = ""Logic.png""/>
              <Filters>
              </Filters>
            </Diagram>";

            var result = DiagramDefinitionParser.ParseDiagramDefinition("", content);
            Assert.IsFalse(result.HideAnonymousLayers);
        }
    }
}
