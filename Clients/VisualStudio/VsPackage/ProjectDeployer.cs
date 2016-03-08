using System;
using System.IO;
using DevArch;
using Microsoft.VisualBasic.FileIO;

namespace ToolsMenu
{
    public static class ProjectDeployer
    {
        private static readonly string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string LocalProjectFilesPath => appdata + "\\CustomProjectSystems\\DevArchProject\\";

        public static void EnsureDevArchProjectSupportExists()
        {
            if (Directory.Exists(LocalProjectFilesPath))
                return;
            // !Directory.Exists(LocalProjectFilesPath)
            Directory.CreateDirectory(LocalProjectFilesPath);
            var dir = Path.GetDirectoryName(new Uri(typeof(ToolsMenuPackage).Assembly.CodeBase).LocalPath) + "\\BuildSystem\\";
            FileSystem.CopyDirectory(dir + "DeployedBuildSystem",LocalProjectFilesPath,true);
            FileSystem.CopyDirectory(dir + "Rules", LocalProjectFilesPath + "Rules\\", true);
        }
    }
}