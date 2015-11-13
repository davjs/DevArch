using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Logic.Building.SemanticTree
{
    public class Tree
    {
        private List<Node> ChildsList { get; } = new List<Node>();
        public IReadOnlyList<Node> Childs => ChildsList;
        public OrientationKind Orientation = OrientationKind.Vertical;

        public override string ToString()
        {
            return string.Join(",", ChildsList);
        }

        public readonly int Id;
        private static int _lastId;

        public Tree()
        {
            Id = _lastId;
            _lastId++;
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

        public void SetChildren(IEnumerable<Node> children)
        {
            var newList = children.ToList();
            ChildsList.Clear();
            AddChilds(newList);
        }

        public void RemoveChild(Node n)
        {
            ChildsList.Remove(n);
        }

        public Node FindNodeWithSymbol(ISymbol symbol)
        {
            return ChildsList.Select(x => x.FindNodeWithSymbol(symbol)).FirstOrDefault(x => x != null);
        }
    }

    public enum OrientationKind
    {
        Horizontal,
        Vertical
    }
}