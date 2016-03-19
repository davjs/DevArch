using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.Build.Construction;

namespace Logic.Integration
{
    public class ArchProject
    {
        private readonly IEnumerable<string> _projectItems;

        public class DiagramDefinitionFile
        {
            private readonly string _path;
            private readonly string _name;
            
            public DiagramDefinitionFile(string path)
            {
                _path = path;
                _name = Path.GetFileNameWithoutExtension(path);
            }

            private string Content => File.ReadAllText(_path);

            public DiagramDefinitionParseResult Parse(string directory)
            {
                try
                {
                    var definition = DiagramDefinitionParser.ParseDiagramDefinition(_name, Content);
                    //Insert directory before output path
                    definition.Output = new OutputSettings(directory + definition.Output.Path,definition.Output.Size);
                    return new DiagramDefinitionParseResult(definition);
                }
                catch (Exception e)
                {
                    return new DiagramDefinitionParseResult(new Exception(_name + "- " + e.Message));
                }
            }
        }

        public ArchProject(ProjectWrapper projectProperties)
        {
            _projectItems = projectProperties.Items.Value;
        }

        public IEnumerable<DiagramDefinitionFile> GetDiagramDefinitionFiles()
        {
            var definitionItems = _projectItems.Where(d => d.EndsWith(".diagramdefinition"));
            return definitionItems.Select(x => new DiagramDefinitionFile(x));
        }
    }
}