using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EnvDTE;
using Logic.Integration;

namespace Logic
{
    public static class ModelDefinitionParser
    {
        public static IEnumerable<ModelDefinition> GetModelDefinitionsFromSolution(AdvancedSolution solution)
        {
            var archProjects = solution.FindArchProjects();
            if (archProjects.Count != 1) throw new NoArchProjectsFound();

            var archProject = archProjects.First();
            var projectItems = archProject.GetAllProjectItems();
            var items =
                projectItems.Where(d => d.Name.EndsWith(".modeldefinition")).ToList();

            var list = new List<ModelDefinition>();
            foreach (var item in items)
            {
                var path = item.FileNames[0];
                var name = item.Name;
                list.Add(ParseModelDefinition(path, name));
            }
            return
                list;
        }

        private static ModelDefinition ParseModelDefinition(string path, string name)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            var modelRoot = xmlDoc["Model"];
            if (modelRoot == null)
                throw new NotImplementedException();

            var scopeHolderNode = modelRoot["Scope"];
            var dependencyAttributeValue = modelRoot.Attributes?.GetNamedItem("DependencyDirection")?.Value;
            var dependencyDown = true;
            if (dependencyAttributeValue != null)
                dependencyDown = dependencyAttributeValue.Equals("Down");
            var scope = ParseScope(scopeHolderNode);
            var output = modelRoot["Output"];
            var outputSettings = ParseOutputSettings(output);
            var filtersNode = modelRoot["Filters"];
            var filters = new Filters();
            if (filtersNode?.ChildNodes != null)
                foreach (XmlNode filter in (filtersNode?.ChildNodes))
                {
                    ParseFilter(filter,ref filters);
                }
            var modelDefinition = new ModelDefinition(name, scope, outputSettings, filters, dependencyDown);
            return modelDefinition;
        }

        private static void ParseFilter(XmlNode filter,ref Filters filters)
        {
            var fname = filter.Name;
            var on = filter.InnerText.Equals("on", StringComparison.CurrentCultureIgnoreCase);
            int number;
            int.TryParse(filter.InnerText, out number);
            if (fname == "RemoveTests") filters.RemoveTests = on;
            if (fname == "RemoveSinglePaths") filters.RemoveSinglePaths = on;
            if (fname == "RemoveExceptions") filters.RemoveExceptions = on;
            if (fname == "FindNamingPatterns") filters.FindNamingPatterns = on;
            if (fname == "MaxDepth") filters.MaxDepth = number;
            if (fname == "MinReferences") filters.MinReferences = number;
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
                default:
                    throw new NotImplementedException();
            }
            if (scope is NamedScope)
                (scope as NamedScope).Name = scopeNode?.Attributes?[0].Value;
            return scope;
        }
    }

    public class NoArchProjectsFound : Exception
    {
    }
}