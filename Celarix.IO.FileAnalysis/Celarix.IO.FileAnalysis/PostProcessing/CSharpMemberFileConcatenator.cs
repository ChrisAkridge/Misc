﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;
using System.IO;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public static class CSharpMemberFileConcatenator
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static void ConcatenateCSharpMemberFiles(string folderPath)
        {
            const string ConcatenatedMemberFileName = "allMembers.cs";
            
            logger.Info("Writing C# member files to single file...");
            
            var cSharpMemberFilePaths = FindAllCSharpMemberFilesInFolder(folderPath)
                .OrderBy(f => f)
                .ToList();
            using var fileStream = new StreamWriter(
                LongFile.OpenWrite(LongPath.Combine(folderPath, ConcatenatedMemberFileName)));
            var writtenLines = 0;

            foreach (var line in cSharpMemberFilePaths
                .Select(filePath => LongFile.ReadAllLines(filePath))
                .SelectMany(l => l))
            {
                fileStream.WriteLine(line);
                writtenLines += 1;

                if (writtenLines % 1000 == 0)
                {
                    logger.Info($"Wrote {writtenLines} lines");
                }
            }
            
            fileStream.Close();

            var deletedFileCount = 0;
            var totalFilesToDelete = cSharpMemberFilePaths.Count;
            foreach (var filePath in cSharpMemberFilePaths)
            {
                LongFile.Delete(filePath);

                deletedFileCount += 1;
                if (deletedFileCount % 100 == 0)
                {
                    logger.Info($"Deleted {deletedFileCount} of {totalFilesToDelete} C# member files");
                }
            }
        }

        private static IEnumerable<string> FindAllCSharpMemberFilesInFolder(string folderPath)
        {
            var cSharpMemberFiles = new List<string>();

            var filesInFolder = LongDirectory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            var foldersInFolder = LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);

            cSharpMemberFiles.AddRange(filesInFolder
                .Where(f => LongPath.GetFileName(f).Equals("members.cs", StringComparison.InvariantCultureIgnoreCase)));

            foreach (var childFolderPath in foldersInFolder)
            {
                cSharpMemberFiles.AddRange(FindAllCSharpMemberFilesInFolder(childFolderPath));
            }

            logger.Info($"Folder {folderPath} has {cSharpMemberFiles.Count:N0} C# member files.");
            return cSharpMemberFiles;
        }
    }
}
