using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using EnvDTE;
using Logic.Filtering.Filters;
using Logic.Integration;
using Logic.Scopes;

namespace Logic
{
    public static class DiagramDefinitionParser
    {
        public static XmlElement RequireTag(this XmlNode doc, string tagName)
        {
            var x = doc[tagName];
            if(x == null)
                throw new Exception($"Unable to find {tagName} tag");
            return x;
        }

        private static string GetAttribute(XmlNode modelRoot, string attributeName)
        {
            var dependencyAttributeValue = modelRoot.Attributes?.GetNamedItem(attributeName)?.Value;
            return dependencyAttributeValue;
        }
        
        public static DiagramDefinition ParseDiagramDefinition(string name,string content)
        {
            var reader = XmlReader.Create(new StringReader(content),new XmlReaderSettings {IgnoreComments = true});
            reader.Read();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);
            
            var modelRoot = xmlDoc.RequireTag("Diagram");
            var scopeHolderNode = modelRoot.RequireTag("Scope");
            var output = modelRoot.RequireTag("Output");
            // Can be null
            var filtersNode = modelRoot["Filters"];

            // Dependency direction
            var dependencyAttributeValue = GetAttribute(modelRoot, "DependencyDirection");
            var dependencyDown = dependencyAttributeValue?.Equals("Down") ?? true;

            var hideAttribute = GetAttribute(modelRoot, "HideAnonymousLayers");
            var hideAnonymous = hideAttribute?.Equals("true") ?? true;

            // Scope
            var scope = ParseScope(scopeHolderNode);

            // OutputSettings
            var outputSettings = ParseOutputSettings(output);

            // Filters
            IEnumerable<Filter> filters = DiagramDefinition.DefaultFilters;
            if (filtersNode?.ChildNodes != null)
            {
                filters = filtersNode.ChildNodes.Cast<XmlNode>().Select(ParseFilter);
            }
            var diagramDefinition = new DiagramDefinition(name, scope, outputSettings, filters, dependencyDown, hideAnonymous);
            return diagramDefinition;
        }

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        private static Filter ParseFilter(XmlNode filterXml)
        {
            var fname = filterXml.Name;

            var filter = DiagramDefinition.DefaultFilters.First(x => x.Name == fname);

            if (filter == null)
                throw new Exception($@"Unrecognized filter: {fname}
                                      Availible filters are{string.Join(",",DiagramDefinition.DefaultFilters)}");

            if (filter is IntegralFilter)
            {
                var integralFilter = filter as IntegralFilter;
                int number;
                if (int.TryParse(filterXml.InnerText, out number) && number >= 0)
                    return integralFilter.WithParameter(number);
                throw new Exception($@"Unrecognized parameter: {filterXml.InnerText}
                                      Parameter for filter {fname} should be positive a number");
            }
            var on = filterXml.InnerText.Equals("on", StringComparison.CurrentCultureIgnoreCase);
            return filter.WithParameter(@on);
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