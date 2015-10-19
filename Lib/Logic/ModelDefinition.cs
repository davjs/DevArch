namespace Logic
{
    public class ModelDefinition
    {
        public string Name;
        public IScope Scope;
        public OutputSettings Output;

        public ModelDefinition(string name, IScope scope, OutputSettings output, Filters filters)
        {
            Name = name;
            Scope = scope;
            Output = output;
            Filters = filters;
        }

        public readonly Filters Filters;

        public static readonly ModelDefinition RootDefault 
            = new ModelDefinition("", new RootScope(), new OutputSettings(), new Filters());
    }

    public class OutputSettings
    {
        public string Path;
        public int Size = 1;
    }
    
    public class Filters
    {
        public bool RemoveTests = true;
        public int ByReference = 1;
        public bool RemoveSinglePaths = true;
    }

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
    public class DocumentScope : NamedScope
    {

    }
}