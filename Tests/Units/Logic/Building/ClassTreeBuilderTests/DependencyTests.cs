using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Logic.Building;
using Logic.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Tests.Units.Logic.Building.ClassTreeBuilderTests
{
    [TestClass]
    public class DependencyTests
    {
        [TestMethod]
        public void ContainsReferences()
        {
            using (var disposable = TestExtesions.BuildProjectTreeFromDocuments("class GuiFacade " +
                    "{" +
                    "public void Foo(){new Button().Bar();}" +
                    "public static void Main(){};" +
                    "}",

                    "class Button {public void Bar(){}}"))
            {
                var projectA = disposable.result;
                var button = projectA.Childs.WithName("Button") as ClassNode;
                Assert.IsNotNull(button);
                Assert.IsTrue(button.References.Any());
            }
        }

        [TestMethod]
        public void ContainsDependencies()
        {
            using (var fakeWorkspace = TestExtesions.BuildProjectTreeFromDocuments(
                "class GuiFacade " +
                    "{" +
                    "public void Foo(){new Button().Bar();}" +
                    "public static void Main(){};" +
                    "}"
                    ,
                    "class Button {public void Bar(){}}"))
            {
                var projectA = fakeWorkspace.result;
                var button = projectA.Childs.WithName("Button") as ClassNode;
                var guiFacade = projectA.Childs.WithName("GuiFacade");
                button.Should().NotBeNull();
                guiFacade.Should().NotBeNull();
                guiFacade.Dependencies.Should().NotBeEmpty();
                //Assert.IsTrue(button.References.Any());
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
            
            var tree = new SolutionNode();
            tree.AddChild(new ProjectNode {Documents = solution.GetProject(pidA).Documents });
            tree.AddChild(new ProjectNode { Documents = solution.GetProject(pidB).Documents });
            ClassTreeBuilder.AddClassesInProjectsToTree(tree);
            var button = tree.DescendantNodes().WithName("Button") as ClassNode;
            var guiFacade = tree.DescendantNodes().WithName("GuiFacade");
            button.Should().NotBeNull();
            guiFacade.Should().NotBeNull();
            guiFacade.Dependencies.Should().NotBeEmpty();
        }
    }
}