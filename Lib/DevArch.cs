using System;
using System.Linq;
using EnvDTE;
using Logic;
using Logic.Integration;
using Presentation;

namespace Lib
{
    public static class DevArch
    {
        public static void RenderAllArchDiagramsToFiles(_DTE enivorment)
        {
            var solution = new AdvancedSolution(enivorment);
            var modelGen = new DiagramFromDiagramDefinitionGenerator(solution);
            var modelDefs = modelGen.GetDiagramDefinitions();
            foreach (var modelDef in modelDefs)
            {
                var tree = modelGen.GenerateDiagram(modelDef);
                if (!tree.Childs.Any())
                    throw new NoClassesFoundException();
                BitmapRenderer.RenderTreeToBitmap(tree,modelDef.DependencyDown, modelDef.Output);
            }
        }

        public static void RenderCompleteDiagramToView(_DTE enivorment,ref ArchView view)
        {
            var solution = new AdvancedSolution(enivorment);
            var modelGen = new DiagramFromDiagramDefinitionGenerator(solution);
            var tree = modelGen.GenerateDiagram(DiagramDefinition.RootDefault);
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree,true);
            view.Diagram.RenderModel(viewModel);
        }
    }

    public class NoClassesFoundException : Exception
    {
    }
}
