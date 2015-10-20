using System;
using System.IO;
using System.Linq;
using EnvDTE;
using Logic;
using Presentation;

namespace Lib
{
    public class DevArch
    {
        public static void RenderAllArchDiagramsToFiles(_DTE enivorment)
        {
            var fullName = GetSolutionName(enivorment);
            
            if (fullName == null)
                throw new NoSolutionOpenException();
            var solutionDir = Path.GetDirectoryName(fullName);
            var modelGen = new DiagramFromModelDefinitionGenerator(enivorment);
            var modelDefs = modelGen.GetModelDefinitions();
            foreach (var modelDef in modelDefs)
            {
                var tree = modelGen.GenerateDiagram(modelDef);
                if (!tree.Childs.Any())
                    throw new NoClassesFoundException();
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
        private static string GetSolutionName(_DTE dte2)
        {
            string name = null;
            while (name == null)
            {
                try
                {
                    name = dte2.Solution.FullName;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return name;
        }
    }

    public class NoClassesFoundException : Exception
    {
    }

    public class NoSolutionOpenException : Exception
    {
    }
}
