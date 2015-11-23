using System;
using System.IO;
using System.Linq;
using EnvDTE;
using Logic;
using Logic.Building;
using Logic.Integration;
using Presentation;

namespace Lib
{
    public class DevArch
    {
        public static void RenderAllArchDiagramsToFiles(_DTE enivorment)
        {
            var solution = new AdvancedSolution(enivorment);
            var modelGen = new DiagramFromModelDefinitionGenerator(solution);
            var solutionDir = solution.Directory();
            var modelDefs = modelGen.GetModelDefinitions();
            foreach (var modelDef in modelDefs)
            {
                var tree = modelGen.GenerateDiagram(modelDef);
                if (!tree.Childs.Any())
                    throw new NoClassesFoundException();
                BitmapRenderer.RenderTreeToBitmap(tree, modelDef.Output);
            }
        }

        public static void RenderCompleteDiagramToView(_DTE enivorment,ref ArchView view)
        {
            var solution = new AdvancedSolution(enivorment);
            var modelGen = new DiagramFromModelDefinitionGenerator(solution);
            var tree = modelGen.GenerateDiagram(ModelDefinition.RootDefault);
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree);
            view.Diagram.RenderModel(viewModel);
        }
    }

    public class NoClassesFoundException : Exception
    {
    }
}
