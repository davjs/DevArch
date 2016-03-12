using System;
using System.Linq;
using FluentAssertions;
using Logic.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Tests.TestExtesions;

namespace Tests.Units.Logic.Building
{
    [TestClass]
    public class ProjectTreeTests
    {
        [TestMethod]
        public void ContainsTopLevelProjectItems()
        {
            var projectItems = new DevArchSolution(TestSolutions.WithSolFolders)
                .SolutionTree.Childs;

            // Assert
            projectItems.Count().Should().Be(3);
            projectItems.Should().ContainSingle(x => x.Name == "FolderA");
            projectItems.Should().ContainSingle(x => x.Name == "FolderB");
            projectItems.Should().ContainSingle(x => x.Name == "ClassLibrary1");
        }

        [TestMethod]
        public void ContainsNestedProjectItems()
        {
            var tree = new DevArchSolution(TestSolutions.WithSolFolders)
                .SolutionTree.Childs;

            // Assert
            var folderA = tree.First();
            var folderAb = folderA.Childs.First();
            var classLibrary1 = folderAb.Childs.First();
            folderA.Name.Should().Be("FolderA");
            folderAb.Name.Should().Be("FolderAB");
            classLibrary1.Name.Should().Be("ClassLibrary1");
        }
    }
}
