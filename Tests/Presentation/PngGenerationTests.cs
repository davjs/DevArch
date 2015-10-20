using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

namespace Analysis.Tests.Presentation
{
    [TestClass]
    public class PngGenerationTests
    {
        private static DTE GetDte()
        {
            return (DTE) Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }

        [TestCategory("PngGeneration")]
        [TestMethod]
        public void GenerateAllArchDiagrams()
        {
            var enviroment = GetDte();
            while (enviroment.Solution == null)
            {
                enviroment = GetDte();
            }
            Lib.DevArch.RenderAllArchDiagramsToFiles(enviroment);
        }
    }
}