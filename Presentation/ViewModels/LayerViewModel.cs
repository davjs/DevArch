using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Orientation = System.Windows.Forms.Orientation;

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
    }

    public class ArchViewModel
    {
        public IEnumerable<LayerViewModel> Layers;
    }
}
