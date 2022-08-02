using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public static class FolderTreePrinter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const int SpacesPerIndentLevel = 2;

        public static void PrintFolderTreeForFolder(string folderPath)
        {
            logger.Info($"Printing tree of all files and folders for {folderPath}...");

            var treeFilePath = LongPath.Combine(folderPath, "tree.txt");
            using var treeFileSteam = new StreamWriter(LongFile.OpenWrite(treeFilePath));
            
            PrintFolderTreeForFolder(folderPath, 0, treeFileSteam);
            treeFileSteam.Close();
        }

        private static void PrintFolderTreeForFolder(string folderPath, int indentLevel, TextWriter stream)
        {
            logger.Info($"Printing tree of depth {indentLevel} of all files and folders for {folderPath}...");
            
            var indent = new string(' ', SpacesPerIndentLevel * indentLevel);
            var childFiles = LongDirectory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            var childFolders = LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var childFolder in childFolders)
            {
                stream.WriteLine($"{indent}{LongPath.GetFileName(childFolder)}");
                PrintFolderTreeForFolder(childFolder, indentLevel + 1, stream);
            }

            foreach (var childFile in childFiles)
            {
                stream.WriteLine($"{indent}{LongPath.GetFileName(childFile)}");
            }
        }
    }
}
