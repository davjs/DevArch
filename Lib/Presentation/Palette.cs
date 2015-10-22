using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Presentation
{
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
