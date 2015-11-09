using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Building;
using Logic.Building.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DependencyTests
    {
       [TestMethod]
        public void ContainsReferences()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From(
                    "class GuiFacade " +
                    "{" +
                    "public void Foo(){new Button().Bar();}" +
                    "public static void Main(){};" +
                    "}"
                    ));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs",
                    SourceText.From("class Button {public void Bar(){}}"));
                var tree = new Tree();
                SemanticTreeBuilder.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                var compile = project.GetCompilationAsync().Result;
                var diagnostics = compile.GetParseDiagnostics();
                Assert.IsTrue(!diagnostics.Any());
                var projectA = tree.Childs.First();
                var button = projectA.Childs.FirstOrDefault(x => x.Name == "Button") as ClassNode;
                Assert.IsNotNull(button);
                Assert.IsTrue(button.References.Any());
            }
        }

        [TestMethod]
        public void ContainsDependencies()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From(
                    "class GuiFacade " +
                    "{" +
                    "public void Foo(){new Button().Bar();}" +
                    "public static void Main(){};" +
                    "}"
                    ));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs",
                    SourceText.From("class Button {public void Bar(){}}"));
                var tree = new Tree();
                SemanticTreeBuilder.AddAllItemsInSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                var projectA = tree.Childs.First();
                var button = projectA.Childs.WithName("Button") as ClassNode;
                var guiFacade = projectA.Childs.WithName("GuiFacade");
                Assert.IsNotNull(button);
                Assert.IsNotNull(guiFacade);
                //Assert.IsTrue(button.References.Any());
                Assert.IsTrue(guiFacade.Dependencies.Any());
            }
        }

        [TestMethod]
        public void ContainsDependenciesFromDifferentProject()
        {
            var pidA = ProjectId.CreateNewId("A");
            var pidB = ProjectId.CreateNewId("B");
            var didA = DocumentId.CreateNewId(pidA);
            var didB = DocumentId.CreateNewId(pidB);
            
            const string docA = @"
            namespace A
            {
                public class Button {public void Bar(){}}
            }            
            ";

            const string docB = @"
            namespace B
            {
                class GuiFacade 
                {
                    public void Foo(){new A.Button().Bar();}
                    public static void Main(){}
                }
            }
            ";
            
            var solution = MSBuildWorkspace.Create(new Dictionary<string, string> { { "CheckForSystemRuntimeDependency", "true" } }).CurrentSolution
            .AddProject(pidA, "ProjectA", "A.dll", LanguageNames.CSharp)
            .AddDocument(didA, "A.cs", docA)
            .AddProject(pidB, "ProjectB", "B.dll", LanguageNames.CSharp)
            .AddDocument(didB, "B.cs", docB)
            .AddProjectReference(pidB, new ProjectReference(pidA));

            var tree = new Tree();
            SemanticTreeBuilder.AddAllItemsInSolutionToTree(solution, ref tree);
            var button = tree.DescendantNodes().WithName("Button") as ClassNode;
            var guiFacade = tree.DescendantNodes().WithName("GuiFacade");
            Assert.IsNotNull(button);
            Assert.IsNotNull(guiFacade);
            Assert.IsTrue(guiFacade.Dependencies.Any());
        }
    }
}