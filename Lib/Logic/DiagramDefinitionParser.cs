using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using EnvDTE;
using Logic.Integration;

namespace Logic
{
    public static class DiagramDefinitionParser
    {
        public static IReadOnlyCollection<DiagramDefinitionParseResult> GetDiagramDefinitionsFromSolution(AdvancedSolution solution)
        {
            var archProjects = solution.FindArchProjects();
            if (archProjects.Count != 1) throw new NoArchProjectsFound();

            var archProject = archProjects.First();
            var projectItems = archProject.GetAllProjectItems();
            var items =
                projectItems.Where(d => d.Name.EndsWith(".diagramdefinition")).ToList();

            var list = new List<DiagramDefinitionParseResult>();
            foreach (var item in items)
            {
                var path = item.FileNames[0];
                var name = item.Name;
                try
                {
                    var definition = ParseDiagramDefinition(path, name);
                    //Insert directory before output path
                    definition.Output.Path = solution.Directory() + definition.Output.Path;
                    list.Add(new DiagramDefinitionParseResult(definition));
                }
                catch (Exception e)
                {
                    list.Add(new DiagramDefinitionParseResult(new Exception(name + "- " + e.Message)));
                }
            }
            return list;
        }

        public static XmlElement RequireTag(this XmlNode doc, string tagName)
        {
            var x = doc[tagName];
            if(x == null)
                throw new Exception($"Unable to find {tagName} tag");
            return x;
        }

        private static DiagramDefinition ParseDiagramDefinition(string path, string name)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            var modelRoot = xmlDoc.RequireTag("Diagram");
            var scopeHolderNode = modelRoot.RequireTag("Scope");
            var output = modelRoot.RequireTag("Output");
            // Can be null
            var filtersNode = modelRoot["Filters"]; 

            // Dependency direction
            var dependencyAttributeValue = modelRoot.Attributes?.GetNamedItem("DependencyDirection")?.Value;
            var dependencyDown = true;
            if (dependencyAttributeValue != null)
                dependencyDown = dependencyAttributeValue.Equals("Down");

            // Scope
            var scope = ParseScope(scopeHolderNode);

            // OutputSettings
            var outputSettings = ParseOutputSettings(output);

            // Filters
            var filters = new Filters();
            if (filtersNode?.ChildNodes != null)
            {
                foreach (XmlNode filter in (filtersNode?.ChildNodes))
                {
                    ParseFilter(filter, ref filters);
                }
            }   
            var diagramDefinition = new DiagramDefinition(name, scope, outputSettings, filters, dependencyDown);
            return diagramDefinition;
        }

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        private static void ParseFilter(XmlNode filter,ref Filters filters)
        {
            var fname = filter.Name;
            var on = filter.InnerText.Equals("on", StringComparison.CurrentCultureIgnoreCase);
            int number;
            int.TryParse(filter.InnerText, out number);
            //Toggles
            if (fname == "RemoveTests") filters.RemoveTests = on;
            else if (fname == "RemoveSinglePaths") filters.RemoveSinglePaths = on;
            else if (fname == "RemoveExceptions") filters.RemoveExceptions = on;
            else if (fname == "FindNamingPatterns") filters.FindNamingPatterns = on;
            else if (fname == "RemoveContainers") filters.RemoveContainers = on;
            // Degrees
            else if (fname == "MaxDepth") filters.MaxDepth = number;
            else if (fname == "MinMethods") filters.MinMethods = number;
            else if (fname == "MinReferences") filters.MinReferences = number;
            else throw new Exception("Unrecognized filter: " + fname);
        }

        private static OutputSettings ParseOutputSettings(XmlNode output)
        {
            var outputPath = output?.Attributes?.GetNamedItem("Path").Value;
            int scale;
            int.TryParse(output?.Attributes?.GetNamedItem("Scale")?.Value, out scale);
            if (scale == 0)
                scale = 1;
            var outputSettings = new OutputSettings()
            {
                Path = outputPath,
                Size = scale
            };
            return outputSettings;
        }

        private static IScope ParseScope(XmlNode scopeHolderNode)
        {
            var scopeNode = scopeHolderNode?.FirstChild;
            IScope scope;
            switch (scopeNode?.Name)
            {
                case "Class":
                    scope = new ClassScope();
                    break;
                case "Document":
                    scope = new DocumentScope();
                    break;
                case "Root":
                    scope = new RootScope();
                    break;
                case "Project":
                    scope = new ProjectScope();
                    break;
                case "Namespace":
                    scope = new NamespaceScope();
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (scope is NamedScope)
                (scope as NamedScope).Name = scopeNode?.Attributes?[0].Value;
            return scope;
        }
    }

    public struct DiagramDefinitionParseResult
    {
        public bool Succeed;
        public DiagramDefinition Definition;
        public Exception Exception;

        public DiagramDefinitionParseResult(Exception exception)
        {
            Exception = exception;
            Succeed = false;
            Definition = null;
        }
        public DiagramDefinitionParseResult(DiagramDefinition definition)
        {
            Definition = definition;
            Succeed = true;
            Exception = null;
        }
    }

    public class NoArchProjectsFound : Exception
    {
    }
}