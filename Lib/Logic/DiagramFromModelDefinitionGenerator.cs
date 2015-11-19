using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Logic.Building;
using Logic.Building.SemanticTree;
using Logic.Filtering;
using Logic.Integration;

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
            return ModelDefinitionParser.GetModelDefinitionsFromSolution(_solution);
        }

        public Tree GenerateDiagram(ModelDefinition modelDef)
        {
            Tree tree = null;
            if (modelDef.Scope is RootScope)
            {
                tree = SemanticTreeBuilder.AnalyseSolution(_solution);
            }
            if (modelDef.Scope is DocumentScope)
            {
                tree = SemanticTreeBuilder.AnalyseDocument(_solution, ((DocumentScope) modelDef.Scope).Name);
            }
            if (modelDef.Scope is ClassScope)
            {
                tree = SemanticTreeBuilder.AnalyseClass(_solution, ((ClassScope)modelDef.Scope).Name);
            }
            if (modelDef.Scope is NamespaceScope)
            {
                tree = SemanticTreeBuilder.AnalyseNamespace(_solution, ((NamespaceScope) modelDef.Scope).Name);
            }
            if (modelDef.Scope is ProjectScope)
            {
                tree = SemanticTreeBuilder.AnalyseProject(_solution, ((ProjectScope) modelDef.Scope).Name);
            }
            ModelFilterer.ApplyFilter(ref tree,modelDef.Filters);

            return modelDef.DependencyDown ? ReverseTree(tree) : tree;
        }

        private static Tree ReverseTree(Tree tree)
        {
            tree.SetChildren(tree.Childs.Select(ReverseTree).Reverse().Cast<Node>());
            return tree;
        }
    }
}
