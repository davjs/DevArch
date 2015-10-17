using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Logic.Analysis;
using Logic.Analysis.SemanticTree;
using Logic.Filtering;
using Microsoft.CodeAnalysis.MSBuild;

namespace Logic
{
    public class DiagramFromModelDefinitionGenerator
    {
        private readonly _DTE _dte;
        private readonly Projects _projects;
        public DiagramFromModelDefinitionGenerator(_DTE dte)
        {
            _dte = dte;
            var solution = GetSolution(dte);
            _projects = solution.Projects;
        }

        public IEnumerable<ModelDefinition> GetModelDefinitions()
        {
            return ModelDefinitionParser.GetModelDefinitionsFromSolution(_projects);
        }


        public Tree GenerateDiagram(ModelDefinition modelDef)
        {
            return GenerateTreeFromModelDefinition(modelDef, _dte, _projects);
        }

        public static Tree GenerateTreeFromModelDefinition(ModelDefinition modeldefinition, _DTE dte, Projects projects)
        {
            Tree tree = null;
            if (modeldefinition.Scope is RootScope)
            {
                tree = Analyser.AnalyseSolution(dte, projects);
            }
            if (modeldefinition.Scope is DocumentScope)
            {
                tree = Analyser.AnalyseDocument(dte,((DocumentScope) modeldefinition.Scope).Name);
            }
            if (modeldefinition.Scope is ClassScope)
            {
                tree = Analyser.AnalyseClass(dte, ((ClassScope)modeldefinition.Scope).Name);
            }
            ModelFilterer.ApplyFilter(ref tree,modeldefinition.Filters);
            return tree;
        }


        private static Solution GetSolution(_DTE dte)
        {
            Solution sol = null;
            while (sol == null)
            {
                try
                {
                    sol = dte.Solution;
                }
                catch (COMException)
                {
                    // ignored
                }
            }
            return sol;
        }

        private static Microsoft.CodeAnalysis.Solution GetSolution(MSBuildWorkspace build, string name)
        {
            Microsoft.CodeAnalysis.Solution solution = null;
            while (solution == null)
            {
                try
                {
                    solution = build.OpenSolutionAsync(name).Result;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return solution;
        }
    }
}
