using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
using Thread = System.Threading.Thread;

namespace Analysis.Tests.Presentation
{
    [TestClass]
    public class PngGenerationTests
    {
        [TestCategory("PngGeneration")]
        [TestMethod]
        public void MakePng()
        {
            var enviroment = GetDte();
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            var path = currentDir.Parent?.Parent?.Parent?.FullName + @"\Current arch.png";
            if(File.Exists(path))
                File.Delete(path);
            BitmapRenderer.RenderArchToBitmap(enviroment, 680, 860, path);
            Assert.IsTrue(File.Exists(path));
        }

        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}