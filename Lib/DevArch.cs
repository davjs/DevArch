using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Logic;
using Logic.Common;
using Logic.Integration;
using Presentation;

namespace Lib
{
    public static class DevArch
    {
        public static async Task RenderAllArchDiagramsToFiles(VisualStudio visualStudio)
        {
            var modelGen = new DiagramFromDiagramDefinitionGenerator(visualStudio.Solution);
            var parseResults = modelGen.GetDiagramDefinitions().ToList();
            var resultLogger = new ParseResultLogger(visualStudio.DevArchOutputWindow(), parseResults);
            resultLogger.PrintErrors();

            var definitions = parseResults.Where(x => x.Succeed).SelectList(x => x.Definition);
            foreach (var modelDef in definitions)
            {
                var tree = modelGen.GenerateDiagram(modelDef);
                if (!tree.Childs.Any())
                    throw new NoClassesFoundException();
                await BitmapRenderer.RenderTreeToBitmapAsync(tree,modelDef.DependencyDown, modelDef.Output, modelDef.HideAnonymousLayers);
                resultLogger.PrintCreated(modelDef.Output.Path);
            }
           resultLogger.PrintSuccess();
        }

        public static void RenderDefaultDiagramDef(VisualStudio visualStudio)
        {
            var diagramGen = new DiagramFromDiagramDefinitionGenerator(visualStudio.Solution);
            var diagramModel = diagramGen.GenerateDiagram(DiagramDefinition.RootDefault);
            BitmapRenderer.RenderTreeToBitmapAsync(diagramModel, true, new OutputSettings(visualStudio.Solution.Directory + "Complete.png")).Wait();
        }

        public static void RenderCompleteDiagramToView(VisualStudio visualStudio, ref ArchView view)
        {
            var modelGen = new DiagramFromDiagramDefinitionGenerator(visualStudio.Solution);
            var tree = modelGen.GenerateDiagram(DiagramDefinition.RootDefault);
            var viewModel = LayerMapper.TreeModelToArchViewModel(tree,true,true);
            view.Diagram.RenderModel(viewModel);
        }
    }

    class ParseResultLogger
    {
        private readonly IReadOnlyCollection<DiagramDefinitionParseResult> _results;
        private readonly OutputWindowPane _output;

        public ParseResultLogger(OutputWindowPane output,IReadOnlyCollection<DiagramDefinitionParseResult> results)
        {
            _results = results;
            _output = output;
            PrintLine("Generating diagrams...");
        }

        void PrintLine(string s)
        {
#if !DEBUG
            _output.OutputString(s + "\n");
#endif
        }

        public void PrintErrors()
        {
            var errorNous = _results.Where(x => !x.Succeed);
            foreach (var result in errorNous)
                PrintLine("Error: " + result.Exception.Message);
            PrintLine("------------------");
        }

        public void PrintCreated(string path)
        {
            PrintLine("Created " + path);
        }
        
        public void PrintSuccess()
        {
            PrintLine("------------------");
            PrintLine($"========== Diagrams: {_results.Count(x => x.Succeed)} Created, " +
                      $"{ _results.Count(x => !x.Succeed)} not created ==========");
        }
    }

    public class NoClassesFoundException : Exception
    {
    }
}
