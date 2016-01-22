using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void RenderAllArchDiagramsToFiles(_DTE enivorment)
        {
            var solution = new AdvancedSolution(enivorment);
            var modelGen = new DiagramFromDiagramDefinitionGenerator(solution);
            var modelDefs = modelGen.GetDiagramDefinitions();
            var resultLogger = new ParseResultLogger(enivorment,modelDefs);
            resultLogger.PrintErrors();

            var definitions = modelDefs.Where(x => x.Succeed).SelectList(x => x.Definition);
            foreach (var modelDef in definitions)
            {
                var tree = modelGen.GenerateDiagram(modelDef);
                if (!tree.Childs.Any())
                    throw new NoClassesFoundException();
                BitmapRenderer.RenderTreeToBitmap(tree,modelDef.DependencyDown, modelDef.Output);
                resultLogger.PrintCreated(modelDef.Output.Path);
            }
            resultLogger.PrintSuccess();
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

    class ParseResultLogger
    {
        private readonly IReadOnlyCollection<DiagramDefinitionParseResult> _results;
        private readonly OutputWindowPane _output;

        public ParseResultLogger(_DTE environment,IReadOnlyCollection<DiagramDefinitionParseResult> results)
        {
            _results = results;
            var dte2 = environment as DTE2;
            if (dte2 == null)
                return;

            var outputWindow = dte2.ToolWindows.OutputWindow;
            _output = outputWindow.OutputWindowPanes.Item("DevArch") ??
                      outputWindow.OutputWindowPanes.Add("DevArch");
            _output.Activate();
            PrintLine("Generating diagrams...");
        }

        void PrintLine(string s)
        {
            _output.OutputString(s + "\n");
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
