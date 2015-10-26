using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Logic.Building;
using Logic.Building.SemanticTree;
using Logic.Filtering;

namespace Logic
{
    public class DiagramFromModelDefinitionGenerator
    {
        private readonly AdvancedSolution _solution;
        public DiagramFromModelDefinitionGenerator(AdvancedSolution solution)
        {
            _solution = solution;
        }

        public IEnumerable<ModelDefinition> GetModelDefinitions()
        {
            return ModelDefinitionParser.GetModelDefinitionsFromSolution(_solution.DteProjects);
        }


        public Tree GenerateDiagram(ModelDefinition modelDef)
        {
            return GenerateTreeFromModelDefinition(modelDef, _solution);
        }

        public static Tree GenerateTreeFromModelDefinition(ModelDefinition modeldefinition, AdvancedSolution solution)
        {
            Tree tree = null;
            if (modeldefinition.Scope is RootScope)
            {
                tree = SemanticTreeBuilder.AnalyseSolution(solution);
            }
            if (modeldefinition.Scope is DocumentScope)
            {
                tree = SemanticTreeBuilder.AnalyseDocument(solution, ((DocumentScope) modeldefinition.Scope).Name);
            }
            if (modeldefinition.Scope is ClassScope)
            {
                tree = SemanticTreeBuilder.AnalyseClass(solution, ((ClassScope)modeldefinition.Scope).Name);
            }
            ModelFilterer.ApplyFilter(ref tree,modeldefinition.Filters);

            return modeldefinition.DependencyDown ? ReverseTree(tree) : tree;
        }

        private static Tree ReverseTree(Tree tree)
        {
            tree.SetChildren(tree.Childs.Select(ReverseTree).Reverse().Cast<Node>());
            return tree;
        }
    }
}
