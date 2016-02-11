using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Logic;
using Logic.SemanticTree;
using Microsoft.VisualStudio.Threading;
using Presentation.ViewModels;
using Thread = System.Threading.Thread;

namespace Presentation
{
    public static class BitmapRenderer
    {

        public static async Task RenderTreeToBitmapAsync(Node tree, bool dependencyDown, OutputSettings outputSettings, bool hideAnonymousNodes = true, bool overWrite = true)
        {
            if (!tree.HasChildren())
                throw new Exception("Empty diagram");

            if (overWrite)
            {
                if (File.Exists(outputSettings.Path))
                    File.Delete(outputSettings.Path);
            }
            
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree,dependencyDown,hideAnonymousNodes);
            await StartStaTask(() => RenderViewModelToBitmap(viewModel, outputSettings.Path, outputSettings.Size));
        }

        public static void RenderTreeToBitmap(Node tree, bool dependencyDown, OutputSettings outputSettings, bool hideAnonymousNodes = true, bool overWrite = true)
        {
            RenderTreeToBitmapAsync(tree, dependencyDown, outputSettings, hideAnonymousNodes, overWrite).Wait();
        }


        private static Task StartStaTask(Action func)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    func();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        private static void RenderViewModelToBitmap(ArchViewModel viewModel,[NotNull] string path, int scale = 1, double maxWidth = double.PositiveInfinity, double maxheight = double.PositiveInfinity)
        {
            var control = new Views.Diagram(viewModel);
            RenderControlToBitmap(control, path,scale, maxWidth, maxheight);
        }

        private static void RenderControlToBitmap(UIElement control, [NotNull] string path, int scale = 1,
            double maxWidth = double.PositiveInfinity, double maxheight = double.PositiveInfinity)
        {
            var availableSize = new Size(maxWidth, maxheight);
            control.Measure(availableSize);
            var desiredSize = control.DesiredSize;
            control.Arrange(new Rect(desiredSize));
            var dpi = scale * 96;
            var bmp = new RenderTargetBitmap((int)desiredSize.Width * scale, (int)desiredSize.Height * scale, dpi, dpi, PixelFormats.Pbgra32);

            bmp.Render(control);

            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bmp));
            var dir = Path.GetDirectoryName(path);
            if (dir == null)
                throw new InvalidPathException(path);
            Directory.CreateDirectory(dir);
            using (Stream stm = File.Create(path))
                encoder.Save(stm);
        }
    }

    internal class InvalidPathException : Exception
    {
        public InvalidPathException(string path) : base(path)
        {
        }
    }
}
