using System.Collections.Generic;
using System.Linq;
using Presentation.Coloring;

namespace Presentation.ViewModels
{
    public class LayerViewModel
    {
        public string Name;
        public IEnumerable<LayerViewModel> Children = new List<LayerViewModel>();
        public int Column;
        public int Row;
        public bool Anonymous;
        public int Rows;
        public int Columns;
        public AdvancedColor Color;
        public int Descendants;


        public override string ToString()
        {
            var childrensToString = string.Join(",",Children);
            return Children.Any() ? $"{Name} = ({childrensToString})" : Name;
        }
    }

    public class ArchViewModel
    {
        public IEnumerable<LayerViewModel> Layers;

        public override string ToString()
        {
            return string.Join(",", Layers.Select(x => x.ToString()));
        }
    }
}
