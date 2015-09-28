using System.Collections.Generic;

namespace Analysis.SemanticTree
{
    public class NodeViewModel : INodeViewModel
    {
        public NodeViewModel(string name)
        {
            Name = name;
        }

        public bool Horizontal { get; set; }
        public string Name { get;}
        public IEnumerable<INodeViewModel> Childs { get; set; }
        public IEnumerable<INodeViewModel> Dependencies { get; set; }
    }
}