using System;

namespace Logic
{
    public class DiagramDefinition
    {
        public string Name;
        public IScope Scope;
        public OutputSettings Output;

        public DiagramDefinition(string name, IScope scope, OutputSettings output, Filters filters, bool dependencyDown = true)
        {
            Name = name;
            Scope = scope;
            Output = output;
            Filters = filters;
            DependencyDown = dependencyDown;
        }

        public readonly Filters Filters;
        public readonly bool DependencyDown;

        public static readonly DiagramDefinition RootDefault 
            = new DiagramDefinition("", new RootScope(), new OutputSettings(), new Filters());
    }

    public class OutputSettings
    {
        public string Path;
        public int Size = 1;
    }
    //TODO: define what filters should be availible aswell as the parsing string and operation for them in a single place
    public class Filters
    {
        public bool RemoveTests = true;
        public int MinReferences = 1;
        public bool RemoveSinglePaths = false;
        public int MaxDepth = 0;
        public bool RemoveDefaultNamespaces = true;
        public bool RemoveExceptions = true;
        public bool FindNamingPatterns = false;
        public int MinMethods = 0;
        public bool RemoveContainers = false;
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