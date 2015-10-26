using System.Collections.Generic;
using System.Linq;
using Presentation.Coloring;
using Colors = Presentation.Coloring.Colors;

namespace Presentation
{

    /*
    public class Hue
    {
        private readonly double _h;
        private readonly double _s;
        private readonly double _l;

        public Hue()
        {
            _h = 0;
            _s = 0.2;
            //TODO: Increase if three has low depth
            _l = 0.02;
        }

        public AdvancedColor GetColor()
        {
            return new AdvancedColor(new Colors.Rgbhsl.Hsl())
            {
                H = _h,
                S = _s,
                L = _l
            };
        }


        private Hue(double h, double s, double l)
        {
            if (h > 1)
                h = h - 1;
            _h = h;
            _s = s;
            _l = l;
        }

        public IEnumerable<Hue> GetDistinctColors(int slices)
        {
            var sizeLeft = 1;
            var sizePerSlice = sizeLeft / (double)slices;
            var newLight = _l * 1.05 + sizeLeft / 12.0;
            if (sizePerSlice < 0.05)
                return Enumerable.Repeat(new Hue(_h, _s, newLight), slices);
            var newHue = _h + sizePerSlice /2.0;
            var ranges = new List<Hue>();
            for (var slice = 0; slice < slices; slice++)
            {
                ranges.Add(new Hue(newHue, _s, newLight));
                newHue += sizePerSlice;
            }
            return ranges;
        }
    }*/

    public class ColorRange
    {
        public readonly double Top;
        public readonly double Bottom;
        private readonly double _s;
        private readonly double _l;
        
        public ColorRange()
        {
            Top = 1;
            Bottom = 0;
            _s = 0.4;
            //TODO: Increase if three has low depth
            _l = 0.03;
        }

        private ColorRange(double top, double bottom, double s, double l)
        {
            Top = top;
            Bottom = bottom;
            _s = s;
            _l = l;
        }

        public AdvancedColor GetColor()
        {
            return new AdvancedColor(new Colors.Rgbhsl.Hsl())
            {
                H = (Bottom + Top) / 2,
                S = _s,
                L = _l
            };
        }

        public IEnumerable<ColorRange> Divide(int slices)
        {
            var sizeLeft = Top - Bottom;
            var sizePerSlice = sizeLeft / slices;
            var newLight = _l*1.05 + sizeLeft / 12;
            if (sizePerSlice < 0.05)
                return Enumerable.Repeat(new ColorRange(Top, Bottom, _s, newLight), slices);
            var newBottom = Bottom;
            var ranges = new List<ColorRange>();
            for (var slice = 0; slice < slices; slice++)
            {
                var newTop = newBottom + sizePerSlice;
                ranges.Add(new ColorRange(newTop,newBottom, _s, newLight));
                newBottom = newTop;
            }
            return ranges;
        }
    }
}
