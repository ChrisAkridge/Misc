using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Extensions;
using NLog;
using LongFile = Pri.LongPath.File;
using LongPath = Pri.LongPath.Path;
using LongDirectory = Pri.LongPath.Directory;
using LongFileInfo = Pri.LongPath.FileInfo;

namespace Celarix.IO.FileAnalysis.StatisticCSVs
{
    public sealed class StatisticCSVWriter : JobPhase
    {
        private const string FilesCSVName = "files.csv";
        private const string FoldersCSVName = "folders.csv";
        private const string FilesCSVHeader = "Full Path,Path Length,Extension,Size";
        private const string FoldersCSVHeader = "Full Path,Path Length,Child Folder Count,Child File Count,Total Size,Average Bytes per File";

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, FolderStatistics> folderStats = new Dictionary<string, FolderStatistics>();
        
        public override AnalysisPhase AnalysisPhase => AnalysisPhase.WritingStatisticCSVs;

        public override void StartOrResume(AnalysisJob job)
        {
            AnalysisJob = job;
            
            logger.Info("(START NEW PHASE) Writing file and folder CSVs...");
            logger.Info("Checking for existing CSVs...");

            var filesCSVAbsolutePath = AnalysisJob.ToAbsolutePath(FileLocation.Output, FilesCSVName);
            var foldersCSVAbsolutePath = AnalysisJob.ToAbsolutePath(FileLocation.Output, FoldersCSVName);

            if (LongFile.Exists(filesCSVAbsolutePath))
            {
                logger.Info("Found files CSV file, deleting...");
                LongFile.Delete(filesCSVAbsolutePath);
            }

            if (LongFile.Exists(foldersCSVAbsolutePath))
            {
                logger.Info("Found folders CSV file, deleting...");
                LongFile.Delete(foldersCSVAbsolutePath);
            }

            WriteFileStatistics(filesCSVAbsolutePath);
            UpdateFolderCounts();
            WriteFolderStatistics(foldersCSVAbsolutePath);
            
            logger.Info("(COMPLETE PHASE) Wrote file and folder CSVs");
        }

        private void WriteFileStatistics(string filesCSVAbsolutePath)
        {
            using var filesCSVWriter = new StreamWriter(LongFile.OpenWrite(filesCSVAbsolutePath));
            filesCSVWriter.WriteLine(FilesCSVHeader);
            var filesWritten = 0;
            
            foreach (var filePath in LongDirectory.EnumerateFiles(AnalysisJob.InputFolderPath, "*",
                SearchOption.AllDirectories))
            {
                logger.Info($"Writing stats for {filePath}");

                try
                {
                    var fileInfo = new LongFileInfo(filePath);
                    var fileStats = GetStatisticsRowForFile(fileInfo);
                    filesCSVWriter.WriteLine(fileStats);
                    filesWritten += 1;
                    AnalysisJob.OriginalFileCount += 1;

                    if (filesWritten == SharedConstants.DefaultBufferCapacity)
                    {
                        logger.Info($"Reached {SharedConstants.DefaultBufferCapacity} files, flushing to disk...");
                        filesCSVWriter.Flush();
                        filesWritten = 0;
                    }

                    AddOrUpdateFolderStatisticsForFile(fileInfo);
                }
                catch (Exception ex) { logger.Error($"Failed to write stats for {filePath}: {JsonSerializer.Serialize(ex)}"); }
            }

            AnalysisJob.SaveJobFile();
        }

        private void WriteFolderStatistics(string foldersCSVAbsolutePath)
        {
            using var foldersCSVWriter = new StreamWriter(LongFile.OpenWrite(foldersCSVAbsolutePath));
            foldersCSVWriter.WriteLine(FoldersCSVHeader);
            var foldersWritten = 0;

            foreach (var (path, stats) in folderStats)
            {
                logger.Info($"Writing stats for {path}");

                try
                {
                    var folderStatsRow = GetStatisticsRowForFolder(stats);
                    foldersCSVWriter.WriteLine(folderStatsRow);
                    foldersWritten += 1;

                    if (foldersWritten == SharedConstants.DefaultBufferCapacity)
                    {
                        logger.Info($"Reached {SharedConstants.DefaultBufferCapacity} files, flushing to disk...");
                        foldersCSVWriter.Flush();
                        foldersWritten = 0;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to write stats for {path}: {JsonSerializer.Serialize(ex)}");
                }
            }
        }

        private static string GetStatisticsRowForFile(LongFileInfo info) =>
            $"{info.FullName},{info.FullName.Length},{LongPath.GetExtension(info.Name).ToLowerInvariant()},{info.Length}";

        private static string GetStatisticsRowForFolder(FolderStatistics stats) =>
            $"{stats.AbsolutePath},{stats.PathLength},{stats.ChildFolderCount},{stats.ChildFileCount},{stats.TotalSize},{stats.AverageFileSize:F2}";

        private void AddOrUpdateFolderStatisticsForFile(LongFileInfo info)
        {
            var folderPath = LongPath.GetDirectoryName(info.FullName);

            AddFolderIfNotExists(folderPath);

            var folderStatsForFile = folderStats[folderPath];
            folderStatsForFile.ChildFileCount += 1;
            folderStatsForFile.TotalSize += info.Length;
            
            AddOrUpdateFolderStatisticsForFolder(folderPath, info.Length);
        }

        private void AddOrUpdateFolderStatisticsForFolder(string folderPath, long additionalSize)
        {
            while (true)
            {
                if (folderPath.WithoutEndingPathSeparator() == AnalysisJob.InputFolderPath.WithoutEndingPathSeparator()) { return; }

                var parentFolderPath = LongPath.GetDirectoryName(folderPath);

                AddFolderIfNotExists(parentFolderPath);

                var folderStatsForParentFolder = folderStats[parentFolderPath];
                folderStatsForParentFolder.ChildFileCount += 1;
                folderStatsForParentFolder.TotalSize += additionalSize;

                folderPath = parentFolderPath;
            }
        }

        private void AddFolderIfNotExists(string folderPath)
        {
            if (!folderStats.ContainsKey(folderPath))
            {
                folderStats.Add(folderPath, new FolderStatistics
                {
                    AbsolutePath = folderPath
                });
            }
        }

        private void UpdateFolderCounts()
        {
            logger.Info($"Writing child folder counts for {folderStats.Count} folders...");

            // Sort the folders by name. All a folder's children will appear immediately
            // after the folder itself. We can then just count until we reach the first
            // folder that isn't a child folder.
            var sortedFolderStats = folderStats.OrderBy(kvp => kvp.Key).ToArray();

            for (var retrievingCountForIndex = 0;
                // Ignore the last folder, as it can't have any children; they'd
                // appear after this folder if there were.
                retrievingCountForIndex < sortedFolderStats.Length - 1;
                retrievingCountForIndex++)
            {
                var childFolderIndex = retrievingCountForIndex + 1;
                var (path, stats) = sortedFolderStats[retrievingCountForIndex];

                while (childFolderIndex < sortedFolderStats.Length && sortedFolderStats[childFolderIndex].Key.Length > path.Length)
                {
                    stats.ChildFolderCount += 1;
                    childFolderIndex += 1;
                }
                
                logger.Info($"{path} has {stats.ChildFolderCount} child folders");
            }
        }
    }
}
