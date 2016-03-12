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
        private readonly Project _project;
        private IEnumerable<string> _projectItems;

        public class DiagramDefinitionFile
        {
            private readonly string _path;
            private readonly string _name;

            public DiagramDefinitionFile(ProjectItem item)
            {
                _path = item.FileNames[0];
                _name = item.Name;
            }
            public DiagramDefinitionFile(string name,string path)
            {
                _path = path;
                _name = name;
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

        public ArchProject(Project project)
        {
            _project = project;
        }

        public ArchProject(ProjectInSolution project)
        {
            _projectItems = new Microsoft.Build.Evaluation.Project(project.AbsolutePath).Items.Select(x => x.EvaluatedInclude);
            
        }

        public IEnumerable<DiagramDefinitionFile> GetDiagramDefinitionFiles()
        {
            _projectItems = _project.GetAllProjectItems().Select(x => x.FileNames[0]);
            var definitionItems = _projectItems.Where(d => d.EndsWith(".diagramdefinition"));
            return definitionItems.Select(x => new DiagramDefinitionFile(x));
        }
    }
}