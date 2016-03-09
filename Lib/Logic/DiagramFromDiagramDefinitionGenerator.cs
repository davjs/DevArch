using System.Collections.Generic;
using System.Linq;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
using Logic.Scopes;
using Logic.SemanticTree;

namespace Logic
{
    public class DiagramFromDiagramDefinitionGenerator
    {
        private readonly DevArchSolution _solution;
        private SolutionNode tree;
        public DiagramFromDiagramDefinitionGenerator(DevArchSolution solution)
        {
            _solution = solution;
        }

        public IEnumerable<DiagramDefinitionParseResult> GetDiagramDefinitions()
        {
            return _solution.ArchProjects.SelectMany(
                project => project.GetDiagramDefinitionFiles().Select(
                    file => file.Parse(_solution.Directory)));
        }

        public Node GenerateDiagram(DiagramDefinition diagramDef)
        {
            if(tree == null)
                tree = SemanticTreeBuilder.AnalyseSolution(_solution);
            Node scoped = null;
            // This boilerplate syntax will look better in C# 7, dont change untill then
            if (diagramDef.Scope is RootScope)
            {
                scoped = tree;
            }
            if (diagramDef.Scope is DocumentScope)
            {
                scoped = SemanticTreeBuilder.AnalyseDocument(tree, ((DocumentScope) diagramDef.Scope).Name);
            }
            if (diagramDef.Scope is ClassScope)
            {
                scoped = SemanticTreeBuilder.AnalyseClass(tree, ((ClassScope)diagramDef.Scope).Name);
            }
            if (diagramDef.Scope is NamespaceScope)
            {
                scoped = SemanticTreeBuilder.AnalyseNamespace(tree, ((NamespaceScope) diagramDef.Scope).Name);
            }
            if (diagramDef.Scope is ProjectScope)
            {
                scoped = SemanticTreeBuilder.AnalyseProject(tree, ((ProjectScope) diagramDef.Scope).Name);
            }

            scoped = scoped.ApplyFilters(diagramDef.Filters)
                .RelayoutBasedOnDependencies();

            return diagramDef.DependencyDown ? ReverseChildren(scoped) : scoped;
        }

        public static Node ReverseChildren(Node tree)
        {
            tree.SetChildren(tree.Orientation == OrientationKind.Horizontal
                ? tree.Childs.Select(ReverseChildren) // Horizontal layers are ordered alphabetically
                : tree.Childs.Select(ReverseChildren).Reverse());
            return tree;
        }
    }
}
