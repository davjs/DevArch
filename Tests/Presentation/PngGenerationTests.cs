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
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var rootDir = currentDir.Parent?.Parent?.Parent?.FullName + @"\";
            var modelGen = new DiagramFromModelDefinitionGenerator(enviroment);
            var modelDefs = modelGen.GetModelDefinitions();
            foreach (var modelDef in modelDefs)
            {
                var tree = modelGen.GenerateDiagram(modelDef);
                var outputPath = rootDir + modelDef.Output.Path;
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                BitmapRenderer.RenderTreeToBitmap(tree,outputPath,modelDef.Output);
                Assert.IsTrue(File.Exists(outputPath));
            }
        }

        /*
                [TestCategory("PngGeneration")]
                [TestMethod]
                public void GenerateSampleFindsAnononymousLayer()
                {
                    var name = "sample1";
                    var enviroment = GetDte();
                    var path = GetFileSlot(name);

                    BitmapRenderer.RenderArchToBitmap(enviroment, 180, 180, path , "FindsAnononymousLayer");
                    Assert.IsTrue(File.Exists(path));
                }

                private static string GetFileSlot(string png)
                {
                    var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                    var path = currentDir.Parent?.Parent?.Parent?.FullName + @"\Samples\" + png + ".png";
                    if (File.Exists(path))
                        File.Delete(path);
                    return path;
                }*/
    }
}