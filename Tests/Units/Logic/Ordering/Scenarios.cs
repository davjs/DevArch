using System.Linq;
using Logic;
using Logic.Filtering;
using Logic.Ordering;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

namespace Tests.Units.Logic.Ordering
{

    [TestClass]
    public class Scenarios
    {
        [TestCategory("Scenarios")]
        [TestMethod]
        public void SoftwareEngineeringModel()
        {
            var nodesList = OrderingTestFactory.CreateNodeList(
                @"
                UiDb ->
                Prog ->
                SWProc ->
                ProjectProg ->
                TAD -> Prog
                LAD -> Prog
                QM -> UiDb, Prog, ProjectProg, SWProc
                ProjectSys -> ProjectProg
                SwArch -> TAD
                Erlang -> ProjectProg
                PPPM -> 
                ProjectDist -> ProjectProg
                TAV -> Prog, Erlang
                Embsys -> Erlang, Prog, LAD
                SPI -> SWProc
                ProjectIndus -> ProjectSys
                CM ->
                MDSD -> Embsys, Erlang, LAD, ProjectSys
                ProjectChange -> UiDb, Prog, ProjectProg, SWProc
                IndustBestPractice -> ProjectSys, ProjectDist, ProjectProg, ProjectIndus
                ResearchMethods ->
                Thesis -> ResearchMethods, ProjectSys, ProjectDist, ProjectProg, ProjectIndus");
            
            var tree = new Node("tree");
            tree.SetChildren(nodesList);
            tree.RelayoutBasedOnDependencies();
            DiagramGenerator.ReverseChildren(tree);
            BitmapRenderer.RenderTreeToBitmap(tree, true, new OutputSettings (TestExtesions.SlnDir + "SEM.png"),false);
        }

        
        [TestCategory("Scenarios")]
        [TestMethod]
        public void CmdModel1()
        {
            var nodesList = OrderingTestFactory.CreateNodeList(
            @"
            ToolsMenuPackage -> CmdFac, GenImCmd
            CmdFac -> CommandBase
            CommandBase ->
            GenImCmd -> CommandBase, DevArch
            ViewDiaCmd -> CommandBase, DevArch
            MainWindow -> DevArch
            DevArch -> 
            ");

            var newList = SiblingReorderer.LayOutSiblingNodes(nodesList);

            var tree = new Node("tree");
            tree.SetChildren(newList);

            DiagramGenerator.ReverseChildren(tree);
            BitmapRenderer.RenderTreeToBitmap(tree, true, new OutputSettings (TestExtesions.SlnDir + "ArchTest.png" ), false);
            Assert.IsFalse(tree.Childs.Last().Childs.Any(x => x.Name == "CommandBase"));
        }

        [TestCategory("Scenarios")]
        [TestMethod]
        public void CmdModel2()
        {
            var nodesList = OrderingTestFactory.CreateNodeList(
            @"
            ToolsMenuPackage -> CmdFac, GenImCmd
            CmdFac -> CommandBase
            CommandBase ->
            GenImCmd -> CommandBase, DevArch
            ViewDiaCmd -> CommandBase, DevArch
            MainWindow -> DevArch
            DevArch -> DiagramDefinition
            DiagramDefinitonParser -> DiagramDefinition
            DiagramDefinition ->
            ");

            var newList = SiblingReorderer.LayOutSiblingNodes(nodesList);

            var tree = new Node("tree");
            tree.SetChildren(newList);

            DiagramGenerator.ReverseChildren(tree);
            BitmapRenderer.RenderTreeToBitmap(tree, true, new OutputSettings (TestExtesions.SlnDir + "ArchTest2.png" ), false);
            Assert.IsFalse(tree.Childs.Last().Childs.Any(x => x.Name == "CommandBase"));
        }
    }
}