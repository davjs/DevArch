using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Lib;
using LibGit2Sharp;
using Logic.Integration;

namespace CommandLineBot
{
    public class Bot
    {
        private DTE _dte;
        [STAThread]
        public static void Main(string[] args)
        {
            new Bot().Run(args[0]);
        }

        public Bot(bool background = true)
        {
            var visualStudioType = Type.GetTypeFromProgID("VisualStudio.DTE.14.0");
            _dte = Activator.CreateInstance(visualStudioType) as DTE;
            if(!background)
                _dte.MainWindow.Activate();
        }

        public void Run(string sourceUrl)
        {
            var dir = Clone(sourceUrl);
            var sln = FindSolution(dir);
            _dte.Solution.Open(sln.FullName);
            _dte.Solution.SolutionBuild.Build(true);
            DevArch.RenderDefaultDiagramDef(new VisualStudio(_dte));
        }

        ~Bot()
        {
            _dte.Quit();
        }

        public static string Clone(string sourceUrl)
        {
            var lastSlash = sourceUrl.LastIndexOf('/');
            var name = sourceUrl.Substring(lastSlash, sourceUrl.Length - lastSlash - 4);
            Repository.IsValid(sourceUrl);
            var dir = "C:/BotRepos" + name + "/";
            if (Directory.Exists(dir))
                return dir;
            Repository.Clone(sourceUrl, dir);
            return dir;
        }

        public static FileInfo FindSolution(string path)
        {
            var solutions =  new DirectoryInfo(path).GetFiles("*.sln", SearchOption.AllDirectories);
            if (solutions.Length == 0)
                throw new Exception("No solution found");
            if (solutions.Length == 1)
                return solutions.First();
            // candidates.Length > 1
            var candidatesByDepth = solutions.GroupBy(x => x.FullName.Count(ch => ch == '/')).ToList();
            var minKey2 = candidatesByDepth.Min(x => x.Key);
            var topLevelCandidates = candidatesByDepth.First(x => x.Key == minKey2);
            if (topLevelCandidates.Count() == 1)
                return topLevelCandidates.First();
            //TopLevelCandidates > 1
            var dirName = Path.GetFileName(path);
            var matchesName = solutions.Where(x => x.Name == dirName).ToList();
            if (matchesName.Count == 1)
                return matchesName.First();
            throw new Exception("Could not find solution");
        }
    }
}
