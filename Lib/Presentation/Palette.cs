using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Presentation
{
    public class Palette
    {
        private readonly List<AdvancedColor> _colors = Colors.KellysMaxContrastSet.Select
            (x => new AdvancedColor(x)).ToList();
        private int _takenColors;
        private AdvancedColor _lastColor;
        public bool HasMoreUniqueColors()
        {
            return _takenColors >= _colors.Count;
        }
        public AdvancedColor GetNextColor()
        {
            _lastColor = _dedicatedColors.Pop();
            _takenColors++;
            return _lastColor;
        }

        private Stack<AdvancedColor> _dedicatedColors = null;

        public void RequestColorsFor(int count)
        {
            if(_takenColors + count > _colors.Count)
            {
                var color = _lastColor.Clone();
                color.L *= 1.1;
                color.S *= 1.2;
                _dedicatedColors = new Stack<AdvancedColor>(Enumerable.Repeat(color, count));
            }
            else
            {
                _dedicatedColors = new Stack<AdvancedColor>(_colors.GetRange(_takenColors, count));
            }
        }

        public Stack<AdvancedColor> RequestSubColorsFor(int count)
        {
            if(count > _colors.Count)
                throw new NotImplementedException();
            return RequestSubColorsFor(null, count);
        }

        public Stack<AdvancedColor> RequestSubColorsFor(AdvancedColor color, int count)
        {
            if (_takenColors + count > _colors.Count)
            {
                var newcolor = color.Clone();
                newcolor.L *= 1.1;
                newcolor.S *= 1.2;
                return new Stack<AdvancedColor>(Enumerable.Repeat(newcolor, count));
            }
            var colors = new Stack<AdvancedColor>(_colors.GetRange(_takenColors, count));
            _takenColors += count;
            return colors;
        }
    }
}
