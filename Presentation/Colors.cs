using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Presentation
{
    public static class Colors
    {

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static IEnumerable<Color> GetNColors(int numColors)
        {
            var list = new List<Color>();
            var random = new Random();

            var ranges = Enumerable.Range(1, numColors);
            var dividedHues = ranges.Select(range => 1d/ range);
            var hslColors = dividedHues.Select(hue =>
                   new Rgbhsl.Hsl
                   {
                       H = hue,
                       S = 0.3,//random.NextDouble().Clamp(0.2, 0.5),
                       L = 0.3//random.NextDouble().Clamp(0.2, 0.3)
                   });

            return hslColors.Select(Rgbhsl.HSL_to_RGB);
        } 

        private static readonly IReadOnlyList<Color> KellysMaxContrastSet = new List<Color>
        {
            UIntToColor(0xFFFFB300), //Vivid Yellow
            UIntToColor(0xFF803E75), //Strong Purple
            UIntToColor(0xFFFF6800), //Vivid Orange
            UIntToColor(0xFFA6BDD7), //Very Light Blue
            UIntToColor(0xFFC10020), //Vivid Red
            UIntToColor(0xFFCEA262), //Grayish Yellow
            UIntToColor(0xFF817066), //Medium Gray

            //The following will not be good for people with defective color vision
            UIntToColor(0xFF007D34), //Vivid Green
            UIntToColor(0xFFF6768E), //Strong Purplish Pink
            UIntToColor(0xFF00538A), //Strong Blue
            UIntToColor(0xFFFF7A5C), //Strong Yellowish Pink
            UIntToColor(0xFF53377A), //Strong Violet
            UIntToColor(0xFFFF8E00), //Vivid Orange Yellow
            UIntToColor(0xFFB32851), //Strong Purplish Red
            UIntToColor(0xFFF4C800), //Vivid Greenish Yellow
            UIntToColor(0xFF7F180D), //Strong Reddish Brown
            UIntToColor(0xFF93AA00), //Vivid Yellowish Green
            UIntToColor(0xFF593315), //Deep Yellowish Brown
            UIntToColor(0xFFF13A13), //Vivid Reddish Orange
            UIntToColor(0xFF232C16) //Dark Olive Green
        };
        
        public static readonly IReadOnlyList<Color> BoyntonOptimized = new List<Color>
        {
            Color.FromArgb(1,0, 0, 255), //Blue
            Color.FromArgb(1,255, 0, 0), //Red
            Color.FromArgb(1,0, 255, 0), //Green
            Color.FromArgb(1,255, 255, 0), //Yellow
            Color.FromArgb(1,255, 0, 255), //Magenta
            Color.FromArgb(1,255, 128, 128), //Pink
            Color.FromArgb(1,128, 128, 128), //Gray
            Color.FromArgb(1,128, 0, 0), //Brown
            Color.FromArgb(1,255, 128, 0) //Orange
        };

        private static Color UIntToColor(uint color)
        {
            var a = (byte) (color >> 24);
            var r = (byte) (color >> 16);
            var g = (byte) (color >> 8);
            var b = (byte) (color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

                /* This tool is part of the xRay Toolkit and is provided free of charge by Bob Powell.
         * This code is not guaranteed to be free from defects or fit for merchantability in any way.
         * By using this tool in your own programs you agree to hold Robert W. Powell free from all
         * damages direct or incidental that arise from such use.
         * You may use this code free of charge in your own projects on condition that you place the
         * following paragraph (enclosed in quotes below) in your applications help or about dialog.
         * "Portions of this code provided by Bob Powell. http://www.bobpowell.net"
         */
        
        public static class Rgbhsl
        {

            public class Hsl
            {
                public Hsl()
                {
                    _h = 0;
                    _s = 0;
                    _l = 0;
                }

                double _h;
                double _s;
                double _l;

                public double H
                {
                    get { return _h; }
                    set
                    {
                        _h = value;
                        _h = _h > 1 ? 1 : _h < 0 ? 0 : _h;
                    }
                }

                public double S
                {
                    get { return _s; }
                    set
                    {
                        _s = value;
                        _s = _s > 1 ? 1 : _s < 0 ? 0 : _s;
                    }
                }

                public double L
                {
                    get { return _l; }
                    set
                    {
                        _l = value;
                        _l = _l > 1 ? 1 : _l < 0 ? 0 : _l;
                    }
                }
            }

            /// <summary>
            /// Converts a colour from HSL to RGB
            /// </summary>
            /// <remarks>Adapted from the algoritm in Foley and Van-Dam</remarks>
            /// <param name="hsl">The HSL value</param>
            /// <returns>A Color structure containing the equivalent RGB values</returns>
            public static Color HSL_to_RGB(Hsl hsl)
            {
                double r = 0, g = 0, b = 0;

                if (hsl.L == 0)
                {
                    r = g = b = 0;
                }
                else
                {
                    if (hsl.S == 0)
                    {
                        r = g = b = hsl.L;
                    }
                    else
                    {
                        var temp2 = ((hsl.L <= 0.5) ? hsl.L * (1.0 + hsl.S) : hsl.L + hsl.S - (hsl.L * hsl.S));
                        var temp1 = 2.0 * hsl.L - temp2;

                        var t3 = new[] { hsl.H + 1.0 / 3.0, hsl.H, hsl.H - 1.0 / 3.0 };
                        var clr = new double[] { 0, 0, 0 };
                        for (var i = 0; i < 3; i++)
                        {
                            if (t3[i] < 0)
                                t3[i] += 1.0;
                            if (t3[i] > 1)
                                t3[i] -= 1.0;

                            if (6.0 * t3[i] < 1.0)
                                clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
                            else if (2.0 * t3[i] < 1.0)
                                clr[i] = temp2;
                            else if (3.0 * t3[i] < 2.0)
                                clr[i] = (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);
                            else
                                clr[i] = temp1;
                        }
                        r = clr[0];
                        g = clr[1];
                        b = clr[2];
                    }
                }

                return Color.FromRgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
            }


            //
            /// <summary>
            /// Converts RGB to HSL
            /// </summary>
            /// <remarks>Takes advantage of whats already built in to .NET by using the Color.GetHue, Color.GetSaturation and Color.GetBrightness methods</remarks>
            /// <param name="c">A Color to convert</param>
            /// <returns>An HSL value</returns>
            public static Hsl RGB_to_HSL(Color c)
            {
                var hsl = new Hsl();
                var c2 = System.Drawing.Color.FromArgb(1, c.R, c.G, c.B);
                hsl.H = c2.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360
                hsl.L = c2.GetBrightness();
                hsl.S = c2.GetSaturation();

                return hsl;
            }
        }
    }
}