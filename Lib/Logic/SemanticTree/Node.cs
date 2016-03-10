using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Logic.Filtering;
using Logic.Integration;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using MoreLinq;
using Document = Microsoft.CodeAnalysis.Document;
using Project = Microsoft.CodeAnalysis.Project;

namespace Logic.SemanticTree
{
    public class Node : UniqueEntity
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

        public readonly ISymbol Symbol;
        public readonly HashSet<Node> Dependencies = new HashSet<Node>();
        public readonly HashSet<Node> References = new HashSet<Node>();
        public HashSet<Node> SiblingDependencies = new HashSet<Node>();
        public Node Parent;
        public OrientationKind Orientation = OrientationKind.Vertical;
        //TODO, try to cash sibling dependencies
        //_indirectSiblingDependencies ?? (_indirectSiblingDependencies = IndirectSiblingBuilder.BuildDepsFor(this));//IndirectSiblingBuilder.BuildDepsFor(this));

        private List<Node> ChildsList { get; } = new List<Node>();
        public IReadOnlyList<Node> Childs => ChildsList;

        public override string ToString()
        {
            return Childs.Any() ? $"{Name} = ({string.Join(",",Childs)})" : Name;
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
            ChildsList.ForEach(n => n.Parent = null);
            ChildsList.Clear();
            AddChilds(newList);
        }

        public void RemoveChild(Node n)
        {
            n.Parent = null;
            ChildsList.Remove(n);
        }

        public int Height()
        {
            if (!Childs.Any())
                return 0;
            return Childs.Max(x => x.Height()) + 1;
        }

        public void FilterChild(Node child)
        {
            Dependencies.UnionWith(child.AllSubDependencies());
            ChildsList.Remove(child);
        }
        public void FilterAllChilds()
        {
            Dependencies.UnionWith(ChildsList.SelectMany(x => x.AllSubDependencies()));
            ChildsList.Clear();
        }
    }

    public class ClassNode : Node
    {
        public readonly IEnumerable<TypeSyntax> BaseClasses;
        public readonly int NrOfMethods;
        public IEnumerable<INamedTypeSymbol> SymbolDependencies;
        public ClassNode(ISymbol symbol, IEnumerable<TypeSyntax> baseClasses, int nrOfMethods) : base(symbol)
        {
            BaseClasses = baseClasses;
            NrOfMethods = nrOfMethods;
        }

        public override bool Equals(object obj)
        {
            if (obj is ClassNode)
                return (obj as ClassNode).Symbol.Equals(Symbol);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Symbol?.GetHashCode() ?? 0;
            }
        }
    }


    public class SolutionNode : Node
    {
        public SolutionNode(string name)
            : base(name) { }
        public SolutionNode()
            : base("Unknown solution name")
        { }
    }

    public class ProjectNode : Node
    {

        public ProjectNode(ProjectItem p) : base(p.Name)
        { }
        public IEnumerable<Document> Documents = new List<Document>();
        public readonly Guid ProjectId;

        public ProjectNode(Project project) : base(project.Name)
        {
            ProjectId = project.Id.Id;
            Documents = project.Documents;
        }

        public ProjectNode(ProjectInSolution solItem) : base(solItem.ProjectName)
        {
            ProjectId = new Guid(solItem.ProjectGuid);
        }
    }

    public class SiblingHolderNode : Node
    {
        public SiblingHolderNode(IEnumerable<Node> siblingNodes, OrientationKind orientation) : base("")
        {
            SetChildren(siblingNodes);
            Orientation = orientation;
        }
    }

    public class HorizontalSiblingHolderNode : SiblingHolderNode
    {
        public HorizontalSiblingHolderNode(IEnumerable<Node> siblingNodes) 
            : base(siblingNodes.OrderBy(x => x.Name),OrientationKind.Horizontal) {}
    }

    public class VerticalSiblingHolderNode : SiblingHolderNode
    {
        public VerticalSiblingHolderNode(IEnumerable<Node> siblingNodes) 
            : base(siblingNodes,OrientationKind.Vertical){}
    }

    public class CircularDependencyHolderNode : SiblingHolderNode
    {
        public CircularDependencyHolderNode(IEnumerable<Node> siblingNodes) 
            : base(siblingNodes,OrientationKind.Horizontal){}
    }
}
 