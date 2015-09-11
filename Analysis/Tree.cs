using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Analysis
{
    public class Tree : ITreeViewModel
    {
        public List<Node> Childs { get; set; } = new List<Node>();
        IEnumerable<INodeViewModel> ITreeViewModel.Childs => Childs;
        public override string ToString()
        {
            return string.Join(",", Childs);
        }

        public Node FindNodeWithSymbol(ISymbol symbol)
        {
            return Childs.Select(x => x.FindNodeWithSymbol(symbol)).FirstOrDefault(x => x != null);
        }

    }

    public interface ITreeViewModel
    {
        IEnumerable<INodeViewModel> Childs { get; }
    }

    public interface INodeViewModel : ITreeViewModel
    {
        string Name { get;}
        IEnumerable<INodeViewModel> Dependencies { get;}
    }

    public class Node : Tree, INodeViewModel
    {
        public Node(ISymbol symbol)
        {
            Symbol = symbol;
        }
        public Node(string name)
        {
            _name = name;
        }

        private readonly string _name;
        public string Name => _name ?? Symbol.Name;

        IEnumerable<INodeViewModel> ITreeViewModel.Childs => Childs;
        IEnumerable<INodeViewModel> INodeViewModel.Dependencies => Childs;

        public ISymbol Symbol;
        public IEnumerable<ReferencedSymbol> References = new List<ReferencedSymbol>();
        public List<Node> Dependencies = new List<Node>();
        public List<Node> InDirectDependencies = new List<Node>();
        public override string ToString()
        {
            return Childs.Any() ? $"({Name} = {base.ToString()})" : Name;
        }
        
        public new Node  FindNodeWithSymbol(ISymbol symbol)
        {
            return Equals(Symbol, symbol) ? this : Childs.Select(x => x.FindNodeWithSymbol(symbol)).FirstOrDefault(x => x != null);
        }
    }

    public class NodeViewModel : INodeViewModel
    {
        public NodeViewModel(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public IEnumerable<INodeViewModel> Childs { get; set; }
        public IEnumerable<INodeViewModel> Dependencies { get; set; }
    }
}