﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    internal static class EmptyFolderRemover
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void RemoveAllEmptyFolders(string folderPath)
        {
            logger.Info($"Removing all empty folders from {folderPath}...");

            if (folderPath.Contains("textMaps/cs", StringComparison.InvariantCultureIgnoreCase)
                || folderPath.Contains("textMaps/asm", StringComparison.InvariantCultureIgnoreCase)
                || folderPath.Contains("textMaps/default", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var foldersInFolder = LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
            var filesInFolder = LongDirectory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var childFolder in foldersInFolder)
            {
                RemoveAllEmptyFolders(childFolder);
            }

            var foldersInFolderAfterRemoval =
                LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
            if (!foldersInFolderAfterRemoval.Any() && !filesInFolder.Any())
            {
                logger.Info($"Folder {folderPath} is empty, removing...");

                try
                {
                    LongDirectory.Delete(folderPath);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to delete {folderPath} ({ex.Message})");
                }
            }
        }
    }
}
