using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Logic;
using Logic.Building.SemanticTree;
using Thread = System.Threading.Thread;

namespace Presentation
{
    public static class BitmapRenderer
    {

        public static void RenderTreeToBitmap(Tree tree, string path, OutputSettings outputSettings)
        {
            var thread = new Thread(() =>
            {
                _RenderTreeToBitmap(tree, path, outputSettings.Size);
            });
            thread.SetApartmentState(ApartmentState.STA); //Make the thread a ui thread
            thread.Start();
            thread.Join();
        }

        private static void _RenderTreeToBitmap(Tree tree, string path, int scale = 1, double maxWidth = double.PositiveInfinity, double maxheight = double.PositiveInfinity)
        {
            var control = new Views.Diagram();
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree);
            control.RenderModel(viewModel);
            RenderControlToBitmap(control, path,scale, maxWidth, maxheight);
        }

        private static void RenderControlToBitmap(UIElement control, string path, int scale = 1,
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

            using (Stream stm = File.Create(path))
                encoder.Save(stm);
        }
    }
}
