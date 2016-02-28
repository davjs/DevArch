using System;
using System.IO;
using CommandLineBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BotTests
{
    [TestClass]
    public class CloneTests
    {
        [TestMethod]
        public void ExistsAfterClone()
        {
            Bot.Clone("https://github.com/davidkron/Flat.git");
            Assert.IsTrue(Directory.Exists("C:/BotRepos/Flat"));
        }


        [TestMethod]
        public void CreatesDiagram()
        {
            var bot = new Bot();
            bot.Run("https://github.com/davidkron/Flat.git");
            Assert.IsTrue(File.Exists(@"C:\BotRepos\Flat\Flat\Complete.png"));
        }


        [TestMethod]
        public void FindsSln()
        {
            Assert.AreEqual(@"C:\BotRepos\Flat\Flat\Flat.sln", Bot.FindSolution(@"C:\BotRepos\Flat").FullName);
        }
    }
}
