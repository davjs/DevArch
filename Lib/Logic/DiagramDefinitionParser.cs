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
using MoreLinq;

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
        
        private static string RequireAttribute(XmlNode modelRoot, string attributeName)
        {
            var value = modelRoot.Attributes?.GetNamedItem(attributeName)?.Value;
            if(string.IsNullOrEmpty(value))
                throw new Exception($"Unable to find {attributeName} under {modelRoot.Name}");
            return value;
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

            filters = AddDefaultFilters(filters);

            var diagramDefinition = new DiagramDefinition(name, scope, outputSettings, filters, dependencyDown, hideAnonymous);
            return diagramDefinition;
        }

        private static IEnumerable<Filter> AddDefaultFilters(IEnumerable<Filter> filters)
        {
            return from defaultFilterSetting in DiagramDefinition.DefaultFilters
                   let usedFilterSetting = filters.FirstOrDefault(x => x.Name == defaultFilterSetting.Name)
                   select usedFilterSetting ?? defaultFilterSetting;
        }

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        private static Filter ParseFilter(XmlNode filterXml)
        {
            var fname = filterXml.Name;

            var filter = DiagramDefinition.DefaultFilters.FirstOrDefault(x => x.Name == fname);

            if (filter == null)
            {
                var defaultFilters = DiagramDefinition.DefaultFilters.Select(x => x.Name);
                throw new Exception($"Unrecognized filter: {fname} \n" +
                                    "Availible filters are:\n" +
                                    $"{string.Join(",", defaultFilters)}");
            }

            if (filter is IntegralFilter)
            {
                var integralFilter = filter as IntegralFilter;
                int number;
                if (int.TryParse(filterXml.InnerText, out number) && number >= 0)
                    return integralFilter.WithParameter(number);
                throw new Exception($"Unrecognized parameter: {filterXml.InnerText}\n" +
                                    $"Parameter for filter {fname} should be positive a number");
            }
            var on = filterXml.InnerText.Equals("on", StringComparison.CurrentCultureIgnoreCase);
            return filter.WithParameter(@on);
        }

        private static OutputSettings ParseOutputSettings(XmlNode output)
        {
            var outputPath = RequireAttribute(output, "Path");
            int scale;
            int.TryParse(output.Attributes?.GetNamedItem("Scale")?.Value, out scale);
            if (scale == 0)
                scale = 1;
            var outputSettings = new OutputSettings(outputPath, scale);
            return outputSettings;
        }

        private static IScope ParseScope(XmlNode scopeHolderNode)
        {
            var scopeNode = scopeHolderNode?.FirstChild;
            var possibleScopes = new List<IScope> {new DocumentScope(),new RootScope(),new ProjectScope()};
            var sname = scopeNode?.Name;
            var scope = possibleScopes.FirstOrDefault(x => x.ParseName() == sname);
            if(scope == null)
            throw new Exception($@"Unrecognized scope: {sname}
                Availible scopes are{string.Join(",", possibleScopes)}");
            if (!(scope is NamedScope)) return scope;
            
            //Scope is NamedScope
            var nameAttribute = scopeNode?.Attributes?[0].Value;
            if(nameAttribute == null)
                throw new Exception($@"Unable to find name attribute for scope: {sname}");
            (scope as NamedScope).Name = nameAttribute;
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
}