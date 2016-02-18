using System.Collections.Generic;
using Logic.Filtering;
using Logic.Filtering.Filters;
using Logic.Scopes;

namespace Logic
{
    public class DiagramDefinition
    {
        public string Name { get; }
        public IScope Scope { get; }
        public OutputSettings Output;
        public IEnumerable<Filter> Filters { get; }
        public bool DependencyDown { get; }
        public bool HideAnonymousLayers { get; }

        public DiagramDefinition(string name, IScope scope, OutputSettings output, IEnumerable<Filter> filters, bool dependencyDown = true, bool hideAnonymousLayers = true)
        {
            Name = name;
            Scope = scope;
            Output = output;
            Filters = filters;
            HideAnonymousLayers = hideAnonymousLayers;
            DependencyDown = dependencyDown;
        }


        public static readonly DiagramDefinition RootDefault 
            = new DiagramDefinition("", new RootScope(), new OutputSettings(""),DefaultFilters);


        public static HashSet<Filter> DefaultFilters => new HashSet<Filter>()
        {
            //On by default
            new RemoveTests(true),
            new RemoveDefaultNamespaces(true),
            new RemoveExceptions(true),
            
            //Availible
            new MaxDepth(0),
            new MinMethods(0)
            //new MinReferences(1),
            //new RemoveSinglePaths (false),
            //new FindNamingPatterns(false),
            //new MinMethods(0),
            //new RemoveContainers(false)
        };
    }



    //TODO: define what filters should be availible aswell as the parsing string and operation for them in a single place
    /*public class Filters
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
    }*/
}