using Logic;
using Logic.Filtering;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;

namespace Tests.Units.Presentation
{
    [TestClass]
    public class Scenarios
    {
        [TestCategory("PngGeneration")]
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
                ");
            
            var tree = new Node("tree");
            tree.SetChildren(nodesList);

            ModelFilterer.ApplyFilter(ref tree, new Filters());
            DiagramFromDiagramDefinitionGenerator.ReverseTree(tree);
            BitmapRenderer.RenderTreeToBitmap(tree, true, new OutputSettings {Path= TestExtesions.SlnDir + "SEM.png"},false);
        }
    }
}