using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Logic;
using Logic.Building.SemanticTree;
using Thread = System.Threading.Thread;

namespace Presentation
{
    public static class BitmapRenderer
    {

        public static void RenderTreeToBitmap(Tree tree, [NotNull] string path, OutputSettings outputSettings)
        {
            Debug.Assert(path != null);
            var thread = new Thread(() =>
            {
                _RenderTreeToBitmap(tree, path, outputSettings.Size);
            });
            thread.SetApartmentState(ApartmentState.STA); //Make the thread a ui thread
            thread.Start();
            thread.Join();
        }

        private static void _RenderTreeToBitmap(Tree tree,[NotNull] string path, int scale = 1, double maxWidth = double.PositiveInfinity, double maxheight = double.PositiveInfinity)
        {
            var control = new Views.Diagram();
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree);
            control.RenderModel(viewModel);
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
