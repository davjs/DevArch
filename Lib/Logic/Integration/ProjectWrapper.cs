using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;

namespace Logic.Integration
{
    //Created to unify lots of project frameworks
    public class ProjectWrapper
    {
        public readonly string Name;
        public readonly string Path ;
        public Lazy<IEnumerable<string>> Items { get; }
        public readonly Guid? Id;
        public bool isLoaded = true;

        //Constructor for EnvDte project

        public ProjectWrapper(EnvDTE.Project project)
        {
            Name = project.Name;
            try
            {
                Path = project.FullName;
                Items = new Lazy<IEnumerable<string>>
                    (() => project.GetAllProjectItems().Select(x => x.FileNames[0]));
            }catch (Exception) //TODO: Handle
            {
                isLoaded = false;
                // Ignored, happens when trying to access the full name of an unloaded project
            }
            // Unsupported
            //ProjectId = null;
        }

        //Constructor for MSBuild project
        public ProjectWrapper(ProjectInSolution project)
        {
            Name = project.ProjectName;
            Path = project.AbsolutePath;
            Items = new Lazy<IEnumerable<string>>
                (() => new Microsoft.Build.Evaluation.Project(project.AbsolutePath).Items.Select(x => x.EvaluatedInclude));
            Id = new Guid(project.ProjectGuid);
        }

    }
}
