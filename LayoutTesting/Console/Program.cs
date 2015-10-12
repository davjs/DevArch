using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Size = System.Windows.Size;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {

            var thread = new Thread(() =>
            {
                _RenderControlToBitmap("Lol.png", new UserControl1());
            });
            thread.SetApartmentState(ApartmentState.STA); //Make the thread a ui thread
            thread.Start();
            thread.Join();
        }


        private static void _RenderControlToBitmap(string path, UIElement control, 
            double maxWidth = double.PositiveInfinity,double maxheight = double.PositiveInfinity)
        {
            var availableSize = new Size(maxWidth, maxheight);
            control.Measure(availableSize);
            var desiredSize = control.DesiredSize;
            control.Arrange(new Rect(desiredSize));
            var bmp = new RenderTargetBitmap((int)desiredSize.Width, (int) desiredSize.Height, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(control);

            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (Stream stm = File.Create(path))
                encoder.Save(stm);
        }
    }
}
