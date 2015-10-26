using System.Collections.Generic;
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
    }

    public class ArchViewModel
    {
        public IEnumerable<LayerViewModel> Layers;
    }
}
