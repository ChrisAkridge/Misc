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
    public static class BinaryDrawingFileRemover
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void RemoveAllBinaryDrawingFiles(string folderPath)
        {
            logger.Info($"Removing all binary drawing files from {folderPath}...");
            var binaryDrawingFilesToRemove = FindAllBinaryDrawingFilesInFolder(folderPath);
            logger.Info($"Found {binaryDrawingFilesToRemove.Count:N0} files to remove.");

            foreach (var filePath in binaryDrawingFilesToRemove)
            {
                LongFile.Delete(filePath);
                logger.Info($"Deleted {filePath}...");
            }
        }

        private static List<string> FindAllBinaryDrawingFilesInFolder(string folderPath)
        {
            var binaryDrawingFilePaths = new List<string>();
            
            var filesInFolder = LongDirectory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            var foldersInFolder = LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
            
            binaryDrawingFilePaths.AddRange(filesInFolder.Where(f => LongPath.GetFileName(f).Equals("bytes.png", StringComparison.InvariantCultureIgnoreCase)));

            foreach (var childFolderPath in foldersInFolder)
            {
                if (LongPath.GetFileName(childFolderPath).Equals("bytes", StringComparison.InvariantCultureIgnoreCase))
                {
                    binaryDrawingFilePaths.AddRange(LongDirectory.GetFiles(childFolderPath, "*",
                        SearchOption.AllDirectories));
                }
                else
                {
                    binaryDrawingFilePaths.AddRange(FindAllBinaryDrawingFilesInFolder(childFolderPath));
                }
            }
            
            logger.Info($"Folder {folderPath} has {binaryDrawingFilePaths.Count:N0} binary drawing files.");
            return binaryDrawingFilePaths;
        }
    }
}
