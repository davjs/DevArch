using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Analysis
{
    public class Tree : ITreeViewModel
    {
        private List<Node> ChildsList { get; set; } = new List<Node>();
        public IReadOnlyList<Node> Childs => ChildsList;
        IEnumerable<INodeViewModel> ITreeViewModel.Childs => ChildsList;
        public override string ToString()
        {
            return string.Join(",", ChildsList);
        }

        public void AddChild(Node childNode)
        {
            childNode.Parent = this;
            ChildsList.Add(childNode);
        }
        
        public void AddChilds(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
                AddChild(node);
        }

        public void UpdateChildren(IEnumerable<Node> children)
        {
            ChildsList = children.ToList();
        }

        public Node FindNodeWithSymbol(ISymbol symbol)
        {
            return ChildsList.Select(x => x.FindNodeWithSymbol(symbol)).FirstOrDefault(x => x != null);
        }

    }

    public static class NodeExtensions
    {
        public static Node WithName(this IEnumerable<Node> nodeList,string name)
        {
            return nodeList.FirstOrDefault(x => x.Name == name);
        }

        public static IEnumerable<Node> Dependencies(this IEnumerable<Node> nodeList)
        {
            return nodeList.SelectMany(x => x.Dependencies);
        }
        public static IEnumerable<Node> SiblingDependencies(this IEnumerable<Node> nodeList)
        {
            return nodeList.SelectMany(x => x.SiblingDependencies);
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


        public readonly ISymbol Symbol;
        public IEnumerable<ReferencedSymbol> References = new List<ReferencedSymbol>();
        public readonly List<Node> Dependencies = new List<Node>();
        public readonly List<Node> SiblingDependencies = new List<Node>();
        public Tree Parent;
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
        public string Name { get;}
        public IEnumerable<INodeViewModel> Childs { get; set; }
        public IEnumerable<INodeViewModel> Dependencies { get; set; }
    }
}