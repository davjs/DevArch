using System.Collections.Generic;

namespace Presentation.ViewModels
{
    public class LayerViewModel
    {
        public string Name;
        public IEnumerable<LayerViewModel> Children;
        public int Column;
        public int Row;
        public bool Anonymous;
        public int Rows;
        public int Columns;
        public AdvancedColor Color;
    }

    public class ArchViewModel
    {
        public IEnumerable<LayerViewModel> Layers;
    }
}
