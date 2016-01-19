﻿using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
using Logic.SemanticTree;

namespace Logic
{
    public class DiagramFromDiagramDefinitionGenerator
    {
        private readonly AdvancedSolution _solution;
        public DiagramFromDiagramDefinitionGenerator(AdvancedSolution solution)
        {
            _solution = solution;
        }

        public IEnumerable<DiagramDefinition> GetDiagramDefinitions()
        {
            return DiagramDefinitionParser.GetDiagramDefinitionsFromSolution(_solution);
        }

        public Node GenerateDiagram(DiagramDefinition modelDef)
        {
            Node tree = null;
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

        private static Node ReverseTree(Node tree)
        {
            tree.SetChildren(tree.Childs.Select(ReverseTree).Reverse());
            return tree;
        }
    }
}