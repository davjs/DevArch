using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnvDTE;
using Thread = System.Threading.Thread;

namespace Presentation
{
    public static class BitmapRenderer
    {
        public static void RenderArchToBitmap(DTE enviroment, int width, int height, string path)
        {
            var thread = new Thread(() =>
            {
                _RenderArchToBitmap(enviroment, width, height, path);
            });
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
        }

        private static void _RenderArchToBitmap(DTE enviroment, int width, int height, string path)
        {
            var control = new ArchView();

            control.GenerateDiagram(enviroment);

            var availableSize = new Size(width, height);
            control.Measure(availableSize);
            control.Arrange(new Rect(availableSize));

            var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(control);

            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (Stream stm = File.Create(path))
                encoder.Save(stm);
        }
    }
}
