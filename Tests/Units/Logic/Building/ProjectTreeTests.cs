﻿using System;
using System.Linq;
using FluentAssertions;
using Logic.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic.Building
{
    [TestClass]
    public class ProjectTreeTests
    {
        [TestMethod]
        public void ContainsTopLevelProjectItems()
        {
            var testDir = AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\TestSolutions\\";
            var sol = testDir + "WithSolFolders\\WithSolFolders.sln";
            var projectItems = DevArchSolution.GetProjectTree(sol).ToList();

            // Assert
            projectItems.Count().Should().Be(3);
            projectItems.Should().ContainSingle(x => x.Name == "FolderA");
            projectItems.Should().ContainSingle(x => x.Name == "FolderB");
            projectItems.Should().ContainSingle(x => x.Name == "ClassLibrary1");
        }

        [TestMethod]
        public void ContainsNestedProjectItems()
        {
            var testDir = AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\TestSolutions\\";
            var sol = testDir + "WithNestedFolders\\WithNestedFolders.sln";
            var tree = DevArchSolution.GetProjectTree(sol);

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