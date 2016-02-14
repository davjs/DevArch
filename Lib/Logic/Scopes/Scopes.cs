namespace Logic.Scopes
{
    public interface IScope
    {
    }

    public class RootScope : IScope
    {
    }

    public class NamedScope : IScope
    {
        public string Name { get; set; }
    }

    public class ClassScope : NamedScope
    {

    }
    public class NamespaceScope : NamedScope
    {

    }
    public class DocumentScope : NamedScope
    {

    }
    public class ProjectScope : NamedScope
    {

    }
}
