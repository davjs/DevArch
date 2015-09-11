using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Solution = Microsoft.CodeAnalysis.Solution;

namespace Analysis
{
    public class Analyser
    {
        public static Tree AnalyseEnviroment(DTE dte2)
        {
            var build = MSBuildWorkspace.Create();
            var name = GetSolutionName(dte2);
            var solution = build.OpenSolutionAsync(name).Result;
            return AnalyzeSolution(solution);
        }

        public static Tree AnalyzeSolution(Solution solution)
        {
            var tree = BuildTreeFromSolution(solution);
            tree = SemanticTreeBuilder.BuildDependenciesFromReferences(tree);
            tree = RemoveSinglePaths(tree);
            tree.Childs = tree.Childs.Select(MoveDependenciesUp).ToList();
            return tree;
        }

        public static Tree BuildTreeFromSolution(Solution solution)
        {
            var projects = solution.Projects.ToList();
            var tree = new Tree();
            if (projects.Count <= 0) return tree;
            var projectTrees = new List<Node>();
            foreach (var project in projects)
            {
                var projectNode = new Node(project.Name);
                var documents = project.Documents.ToList();
                if (documents.Any())
                {
                    var semanticModels = documents.Select(d => d.GetSemanticModelAsync().Result).ToList();
                    var analysis =
                        semanticModels.SelectMany(model => GetClassesInModel(model, solution)).ToList();
                    var classes = analysis;
                    if (!classes.Any())
                        throw new Exception("No classes found");
                    var classnodes = SemanticTreeBuilder.BuildTreeFromClasses(classes);
                    projectNode.Childs.AddRange(classnodes);
                }
                projectTrees.Add(projectNode);
            }
            tree.Childs.AddRange(projectTrees);
            return tree;
        }

        public static Node MoveDependenciesUp(Node node)
        {
            var childs = node.Childs.Select(MoveDependenciesUp);
            node.InDirectDependencies = node.Dependencies.Concat(childs.SelectMany(c => c.InDirectDependencies.Concat(c.Dependencies))).ToList();
            return node;
        }

        private static Tree RemoveSinglePaths(Tree tree)
        {
            tree.Childs = tree.Childs.Select(RemoveSinglePaths).Cast<Node>().ToList();
            return tree.Childs.Count == 1 ? tree.Childs.First() : tree;
        }

        /*public class NameSpace
        {
            public string Name;
            public IEnumerable<string> Parents;
        }

        public static NameSpace SplitNameSpaceName(string name)
        {
            if (!name.Contains(".")) return new NameSpace {Name = name, Parents = new List<string>()};
            var names = name.Split('.');
            var parents = names.Take(names.Count() - 1);
            return new NameSpace { Name = names.First(), Parents = parents };
        }*/

        private static IEnumerable<ClassInfo> GetClassesInModel(SemanticModel model, Solution solution)
        {
            var syntaxRoot = model.SyntaxTree.GetRootAsync().Result;
            var classes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var classAnalysis = classes.Select(c => AnalyseClass(model, c, solution)).ToList();
            return classAnalysis;
        }

        private static ClassInfo AnalyseClass(SemanticModel model, ClassDeclarationSyntax c, Solution solution)
        {
            //var typeInfo = model.GetTypeInfo(c).Type;
            var declaredSymbol = model.GetDeclaredSymbol(c);
            var references = SymbolFinder.FindReferencesAsync(declaredSymbol,solution ).Result;
            var namespaceSymbol = declaredSymbol.ContainingNamespace;
            return new ClassInfo { NameSpace = namespaceSymbol,References = references,Symbol = declaredSymbol };
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
}
