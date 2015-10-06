using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Analysis.SemanticTree
{
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
        public readonly HashSet<Node> SiblingDependencies = new HashSet<Node>();
        public Tree Parent;
        public IEnumerable<TypeSyntax> BaseClasses = new List<TypeSyntax>();

        public Node(ClassInfo @class)
        {
            Symbol = @class.Symbol;
            References = @class.References;
            BaseClasses = @class.BaseTypes;
        }

        public override string ToString()
        {
            return Childs.Any() ? $"({Name} = {base.ToString()})" : Name;
        }
        
        public new Node  FindNodeWithSymbol(ISymbol symbol)
        {
            return Equals(Symbol, symbol) ? this : Childs.Select(x => x.FindNodeWithSymbol(symbol)).FirstOrDefault(x => x != null);
        }
    }
}