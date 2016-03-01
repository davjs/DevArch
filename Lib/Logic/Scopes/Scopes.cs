namespace Logic.Scopes
{
    public interface IScope
    {
        string ParseName();
    }

    public class RootScope : IScope
    {
        public string ParseName() => "Root";
    }

    public abstract class NamedScope : IScope
    {
        public string Name { get; set; }
        public abstract string ParseName();
    }

    public class ClassScope : NamedScope
    {
        public override string ParseName() => "Class";
    }
    public class NamespaceScope : NamedScope
    {
        public override string ParseName() => "Namespace";
    }
    public class DocumentScope : NamedScope
    {
        public override string ParseName() => "Document";
    }
    public class ProjectScope : NamedScope
    {
        public override string ParseName() => "Project";
    }
}
