using System;
using System.IO;
using Presentation;
using EnvDTE;
using Logic;

namespace DevArch.lib
{
    public static class DevArch
    {
        public static void RenderAllArchDiagramsToFiles(_DTE enivorment)
        {
            var fullName = enivorment.Solution?.FullName;
            if (fullName == null)
                throw new NoSolutionOpenException();
            var solutionDir = Path.GetDirectoryName(fullName);
            var modelGen = new DiagramFromModelDefinitionGenerator(enivorment);
            var modelDefs = modelGen.GetModelDefinitions();
            foreach (var modelDef in modelDefs)
            {
                var tree = modelGen.GenerateDiagram(modelDef);
                var outputPath = solutionDir + "\\" + modelDef.Output.Path;
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                BitmapRenderer.RenderTreeToBitmap(tree, outputPath, modelDef.Output);
            }
        }

        public static void RenderCompleteDiagramToView(_DTE enivorment,ref ArchView view)
        {
            var modelGen = new DiagramFromModelDefinitionGenerator(enivorment);
            var tree = modelGen.GenerateDiagram(ModelDefinition.RootDefault);
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree);
            view.Diagram.RenderModel(viewModel);
        }
    }

    public class NoSolutionOpenException : Exception
    {
    }
}
