using System.Collections.Generic;
using System.Linq;
using Logic.SemanticTree;
using Presentation.Coloring;

namespace Presentation.ViewModels
{
    public class LayerViewModel : DiagramSymbolViewModel
    {
        public string Name;
        public IEnumerable<DiagramSymbolViewModel> Children = new List<DiagramSymbolViewModel>();
        public bool Anonymous;
        public bool Invisible;
        public int Rows;
        public int Columns;
        public AdvancedColor Color;
        public int Descendants;
        public OrientationKind Orientation;


        public override string ToString()
        {
            var childrensToString = string.Join(",",Children);
            return Children.Any() ? $"{Name} = ({childrensToString})" : Name;
        }
    }


    public class ArchViewModel
    {
        public IEnumerable<DiagramSymbolViewModel> Layers;

        public override string ToString()
        {
            return string.Join(",", Layers.Select(x => x.ToString()));
        }
    }
}
