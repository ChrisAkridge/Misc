using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    internal static class FileListGenerator
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static string[] GenerateFileList(string folderPath)
        {
            logger.Info($"Generating list of all files in {folderPath}...");

            var filePaths = GetAllFilesInFolder(folderPath);

            return filePaths
                .OrderBy(f => f)
                .ToArray();
        }

        private static IEnumerable<string> GetAllFilesInFolder(string folderPath)
        {
            var filePaths = new List<string>();
            
            var filesInFolder = LongDirectory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            var foldersInFolder = LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
            
            filePaths.AddRange(filesInFolder);

            foreach (var childFolderPath in foldersInFolder)
            {
                filePaths.AddRange(GetAllFilesInFolder(childFolderPath));
            }
            
            logger.Info($"Folder {folderPath} has {filePaths.Count:N0} files.");
            return filePaths;
        }
    }
}
