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

        public static void RemoveAllBinaryDrawingFiles(string[] filePaths)
        {
            logger.Info($"Removing all binary drawing files...");
            var binaryDrawingFilesToRemove = FindAllBinaryDrawingFilesInFolder(filePaths);
            logger.Info($"Found {binaryDrawingFilesToRemove.Count:N0} files to remove.");

            foreach (var filePath in binaryDrawingFilesToRemove)
            {
                LongFile.Delete(filePath);
                logger.Info($"Deleted {filePath}...");
            }
        }

        private static List<string> FindAllBinaryDrawingFilesInFolder(string[] filePaths)
        {
            var binaryDrawingFilePaths = new List<string>();
            var filesCountedSoFar = 0;

            foreach (var path in filePaths)
            {
                if (LongPath.GetFileName(path).Equals("bytes.png", StringComparison.InvariantCultureIgnoreCase) || FileIsPartOfCanvas(path))
                {
                    binaryDrawingFilePaths.Add(path);
                }

                filesCountedSoFar += 1;

                if (filesCountedSoFar % 1000 == 0)
                {
                    logger.Info($"Searched for binary drawing files across {filesCountedSoFar:N0} files");
                }
            }

            logger.Info($"Found {binaryDrawingFilePaths.Count:N0} binary drawing files.");
            return binaryDrawingFilePaths;
        }

        private static bool FileIsPartOfCanvas(string filePath)
        {
            if (!LongPath.GetExtension(filePath)
                    .Contains("png", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var pathParts = filePath.Split(LongPath.DirectorySeparatorChar);
            var indexOfBytesPart = Array.IndexOf(pathParts, "bytes");

            if (indexOfBytesPart == -1)
            {
                return false;
            }

            for (int i = indexOfBytesPart + 1; i < pathParts.Length; i++)
            {
                if (!LongPath.GetFileNameWithoutExtension(pathParts[i]).All(c => char.IsDigit(c)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
