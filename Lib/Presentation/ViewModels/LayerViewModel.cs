using System.Collections.Generic;
using System.Linq;
using Presentation.Coloring;

namespace Presentation.ViewModels
{
    public interface IDiagramSymbolViewModel
    {

    }

    public class LayerViewModel : IDiagramSymbolViewModel
    {
        public string Name;
        public IEnumerable<IDiagramSymbolViewModel> Children = new List<IDiagramSymbolViewModel>();
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


    public class ArrowViewModel : IDiagramSymbolViewModel
    {
        enum Direction
        {
            Left,Right,Up,Down
        }
        private Direction direction;
    }

    public class ArchViewModel
    {
        public IEnumerable<IDiagramSymbolViewModel> Layers;

        public override string ToString()
        {
            return string.Join(",", Layers.Select(x => x.ToString()));
        }
    }
}
