using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;
using System.IO;
using Celarix.IO.FileAnalysis.Utilities;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public static class CSharpMemberFileConcatenator
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static void ConcatenateCSharpMemberFiles(string folderPath, string[] filePaths)
        {
            const string ConcatenatedMemberFileName = "allMembers.cs";
            
            logger.Info("Writing C# member files to single file...");

            var cSharpMemberFilePaths = FindAllCSharpMemberFilesInFolder(filePaths);
            using var fileStream = new StreamWriter(
                LongFile.OpenWrite(LongPath.Combine(folderPath, ConcatenatedMemberFileName)));
            var writtenLines = 0;
            var fileProgress = new AdvancedProgress(cSharpMemberFilePaths.Length, DateTimeOffset.Now);

            foreach (var filePath in cSharpMemberFilePaths)
            {
                foreach (var line in LongFile.ReadAllLines(filePath))
                {
                    fileStream.WriteLine(line);
                    writtenLines += 1;

                    if (writtenLines % 1000 == 0) { logger.Info($"Wrote {writtenLines} lines"); }
                }

                fileProgress.CurrentAmount += 1;
                logger.Info(fileProgress.ToString());
            }

            fileStream.Close();

            var totalFilesToDelete = cSharpMemberFilePaths.Length;
            var deletionProgress = new AdvancedProgress(totalFilesToDelete, DateTimeOffset.Now);

            foreach (var filePath in cSharpMemberFilePaths)
            {
                LongFile.Delete(filePath);

                deletionProgress.CurrentAmount += 1;
                if (deletionProgress.CurrentAmount % 100 == 0)
                {
                    logger.Info(deletionProgress.ToString());
                }
            }
        }

        private static string[] FindAllCSharpMemberFilesInFolder(string[] filePaths)
        {
            var cSharpMemberFilePaths = filePaths
                .Where(f => LongPath.GetFileName(f).Equals("members.cs", StringComparison.InvariantCultureIgnoreCase)
                    && !f.Contains("textMaps\\cs", StringComparison.InvariantCultureIgnoreCase)
                    && !f.Contains("textMaps\\asm", StringComparison.InvariantCultureIgnoreCase)
                    && !f.Contains("textMaps\\default", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            logger.Info($"Found {cSharpMemberFilePaths.Length:N0} C# member files.");
            return cSharpMemberFilePaths;
        }
    }
}
