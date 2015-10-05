using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Document = Microsoft.CodeAnalysis.Document;
using Project = Microsoft.CodeAnalysis.Project;

namespace Analysis.SemanticTree
{
    public class Tree : ITreeViewModel
    {
        private List<Node> ChildsList { get; } = new List<Node>();
        public IReadOnlyList<Node> Childs => ChildsList;
        IEnumerable<INodeViewModel> ITreeViewModel.Childs => ChildsList;
        public bool Horizontal { get; set; }
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

        public void UpdateChildren(IEnumerable<Node> children)
        {
            var newList = children.ToList();
            ChildsList.Clear();
            AddChilds(newList);
        }

        public Node FindNodeWithSymbol(ISymbol symbol)
        {
            return ChildsList.Select(x => x.FindNodeWithSymbol(symbol)).FirstOrDefault(x => x != null);
        }
    }

    public interface ITreeViewModel
    {
        IEnumerable<INodeViewModel> Childs { get; }
    }

    public interface INodeViewModel : ITreeViewModel
    {
        bool Horizontal { get; set; }
        string Name { get;}
        IEnumerable<INodeViewModel> Dependencies { get;}
    }

    public class ProjectNode : Node
    {
        public ProjectNode(ProjectItem p) : base(p.Name)
        { }
        public IEnumerable<Document> Documents = new List<Document>();

        public ProjectNode(Project project) : base(project.Name)
        {
            Documents = project.Documents;
        }
    }

    public class SiblingHolderNode : Node
    {
       public SiblingHolderNode(IEnumerable<Node> siblingNodes) : base("")
        {
            UpdateChildren(siblingNodes);
            Horizontal = true;
        }
    }

    public class CircularDependencyHolderNode : SiblingHolderNode
    {
        public CircularDependencyHolderNode(IEnumerable<Node> siblingNodes) : base(siblingNodes)
        {
        }
    }

}