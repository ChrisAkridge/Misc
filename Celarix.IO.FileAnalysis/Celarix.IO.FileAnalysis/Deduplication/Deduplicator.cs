using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Extensions;
using Celarix.IO.FileAnalysis.Utilities;
using CSharpLib;
using NLog;
using LongFile = Pri.LongPath.File;

namespace Celarix.IO.FileAnalysis.Deduplication
{
    public sealed class Deduplicator : JobPhase
    {
        private const string HashFileSortedTempPath = "job\\deduplicate\\hashes_sorted.txt";
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private AdvancedProgress advancedProgress;
        
        public override AnalysisPhase AnalysisPhase => AnalysisPhase.DeduplicatingFiles;

        public override void StartOrResume(AnalysisJob job)
        {
            AnalysisJob = job;
            advancedProgress = new AdvancedProgress(AnalysisJob.OriginalFileCount, AnalysisJob.PhaseStartedOn);

            logger.Info("(START NEW PHASE) Deduplicating files...");
            CallWindowsSort();
            DeduplicateFiles();
        }

        private void CallWindowsSort()
        {
            logger.Info("Sorting hash file...");
            
            var sortedHashFilePath = AnalysisJob.ToAbsolutePath(FileLocation.Output, HashFileSortedTempPath);
            var hashFilePath = AnalysisJob.ToAbsolutePath(FileLocation.Output, SharedConstants.HashFilePath);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "C:\\Windows\\system32\\sort.exe",
                Arguments = $"\"{hashFilePath}\" /o \"{sortedHashFilePath}\"",
                RedirectStandardOutput = true
            };

            var sortProcess = Process.Start(processStartInfo);
            sortProcess.WaitForExit();

            logger.Info("Sorted hash file");

            LongFile.Delete(hashFilePath);
            LongFile.Move(sortedHashFilePath, hashFilePath);
        }

        private void DeduplicateFiles()
        {
            using var hashFileStream =
                new StreamReader(LongFile.OpenRead(AnalysisJob.ToAbsolutePath(FileLocation.Output, SharedConstants.HashFilePath)));

            var duplicateGroups = EnumerateHashes(hashFileStream)
                .GroupOrderedBy(t => t.hash);
            var foundDuplicates = 0;

            foreach (var groupEnumerator in duplicateGroups.Select(g => g.GetEnumerator()))
            {
                groupEnumerator.MoveNext();
                
                var (_, canonicalFilePath) = groupEnumerator.Current;

                while (groupEnumerator.MoveNext())
                {
                    DeleteFileAndCreateShortcut(canonicalFilePath, groupEnumerator.Current.filePath);
                    foundDuplicates++;
                }

                groupEnumerator.Dispose();
            }
            
            logger.Info($"Deduplication complete, found {foundDuplicates} duplicates");
        }
        
        private void DeleteFileAndCreateShortcut(string canonicalFilePath, string duplicateToDeletePath)
        {
            logger.Info($"{duplicateToDeletePath} is a duplicate of {canonicalFilePath}! Deleting...");

            try
            {
                LongFile.Delete(duplicateToDeletePath);

                var shortcut = new Shortcut();
                shortcut.CreateShortcutToFile(canonicalFilePath, duplicateToDeletePath + ".lnk");
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
        }

        private IEnumerable<(string hash, string filePath)> EnumerateHashes(StreamReader hashFileReader)
        {
            while (!hashFileReader.EndOfStream)
            {
                var hashFileLine = hashFileReader.ReadLine();
                // https://stackoverflow.com/a/21519598/2709212
                var hashFileComponents = hashFileLine.Split(new[] { ':' }, 2);

                advancedProgress.CurrentAmount += 1;
                logger.Info(advancedProgress.ToString());

                yield return (hashFileComponents[0].TrimEnd(), hashFileComponents[1].TrimStart());
            }
        }
    }
}
