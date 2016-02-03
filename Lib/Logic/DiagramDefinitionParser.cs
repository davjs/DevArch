using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using EnvDTE;
using Logic.Integration;

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
            var filters = new Filters();
            if (filtersNode?.ChildNodes != null)
            {
                foreach (XmlNode filter in (filtersNode?.ChildNodes))
                {
                    ParseFilter(filter, ref filters);
                }
            }   
            var diagramDefinition = new DiagramDefinition(name, scope, outputSettings, filters, dependencyDown, hideAnonymous);
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