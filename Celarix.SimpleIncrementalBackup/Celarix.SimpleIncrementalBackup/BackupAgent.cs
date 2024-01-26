using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.SimpleIncrementalBackup.Data;
using Celarix.SimpleIncrementalBackup.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NLog;

namespace Celarix.SimpleIncrementalBackup
{
    public sealed class BackupAgent(string sourceFolderPath, string backupFolderPath, bool deleteAllDeletedFilesNow = false) : IDisposable
    {
        private const int SaveChangesInterval = 1000;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private BackupContext? context;
        private int newFileCount;
        private int deletedFileCount;
        private long sizeDelta;
        private int modifiedRowCount;

        public string SourceFolderPath { get; } = sourceFolderPath;
        public string BackupFolderPath { get; } = backupFolderPath;
        public bool DeleteAllDeletedFilesNow { get; } = deleteAllDeletedFilesNow;

        public void RunBackup()
        {
            LoggingConfigurer.ConfigureConsoleLogging();
            logger.Info($"Starting backup from {SourceFolderPath} to {BackupFolderPath}");

            var databasePath = Path.Combine(BackupFolderPath, "backupLog.db");
            context = new BackupContext(databasePath);
            context.Database.EnsureCreated();
            logger.Info("Database created if not present");

            var backupLog = GetLogForCurrentBackup(context);
            var seenFilePaths = new List<string>();

            CopyFilesAndUpdateDatabase(seenFilePaths);

            logger.Info($"Scanning database for deleted files, saw {seenFilePaths.Count} files in source.");

            MarkOrDeleteFilesNotInSource(seenFilePaths);

            backupLog.EndTime = DateTimeOffset.Now;
            backupLog.NewFileCount = newFileCount;
            backupLog.DeletedFileCount = deletedFileCount;
            backupLog.SizeDelta = sizeDelta;
            context.SaveChanges();

            logger.Info($"Backup complete. {newFileCount} new files, {deletedFileCount} deleted files, {GetFileSizeString(sizeDelta)} bytes changed");
        }

        private void CopyFilesAndUpdateDatabase(List<string> seenFilePaths)
        {
            var sourceFiles = Directory.EnumerateFiles(SourceFolderPath, "*", SearchOption.AllDirectories);
            foreach (var sourceFilePath in sourceFiles)
            {
                logger.Info($"Processing {sourceFilePath}...");
                seenFilePaths.Add(sourceFilePath);
                var sourceFileInfo = new FileInfo(sourceFilePath);

                var entryFilePath = GetEntryFilePath(sourceFilePath);
                var fileEntry = GetFileEntryFromDatabase(entryFilePath) ?? AddFileToDatabase(sourceFileInfo);

                var destinationFilePath = GetDestinationFilePath(entryFilePath);
                if (!File.Exists(destinationFilePath) || sourceFileInfo.LastWriteTimeUtc > fileEntry.BackupLastUpdated)
                {
                    logger.Info($"Copying {sourceFilePath} to {destinationFilePath} ({GetFileSizeString(sourceFileInfo.Length)})...");
                    File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                    UpdateFileEntryTimestamps(fileEntry, sourceFileInfo.LastWriteTimeUtc, DateTimeOffset.Now);

                    newFileCount += 1;
                    sizeDelta += sourceFileInfo.Length;
                }
            }
        }

        private void MarkOrDeleteFilesNotInSource(List<string> seenFilePaths)
        {
            context!.SaveChanges();
            var allFilesInDatabase = context!.FileEntries
                .ToList()
                .Select(e => Path.Combine(SourceFolderPath, e.Path));
            var allFilesNotInSource = allFilesInDatabase.Except(seenFilePaths);

            foreach (var deletedFilePath in allFilesNotInSource)
            {
                // Should be fast enough since the deleted files shouldn't be too common.
                var entryFilePath = GetEntryFilePath(deletedFilePath);
                var matchingEntry = context.FileEntries.FirstOrDefault(e => e.Path == entryFilePath);
                if (DeleteAllDeletedFilesNow || matchingEntry!.FileDeletedOnLastBackup)
                {
                    logger.Warn($"Deleting {deletedFilePath} from backup folder!");
                    var fileToDeletePath = Path.Combine(BackupFolderPath, entryFilePath);
                    var deletedFileSize = new FileInfo(fileToDeletePath).Length;
                    File.Delete(fileToDeletePath);
                    deletedFileCount += 1;
                    sizeDelta -= deletedFileSize;
                }
                else
                {
                    MarkFileAsDeletedFromSource(matchingEntry);
                }
            }
        }

        private static BackupLog GetLogForCurrentBackup(BackupContext context)
        {
            var currentBackupLog = new BackupLog
            {
                StartTime = DateTimeOffset.Now,
            };
            var logEntry = context.BackupLogs.Add(currentBackupLog);
            context.SaveChanges();
            return logEntry.Entity;
        }

        private string GetEntryFilePath(string filePath)
        {
            // Given C:/files/file.txt and a source file root of C:/files, return file.txt
            var relativePath = filePath.Substring(SourceFolderPath.Length);
            return relativePath.TrimStart('\\');
        }

        private string GetDestinationFilePath(string entryFilePath)
        {
            return Path.Combine(BackupFolderPath, entryFilePath);
        }

        private FileEntry? GetFileEntryFromDatabase(string entryFilePath) => context!.FileEntries.FirstOrDefault(f => f.Path == entryFilePath);

        private FileEntry AddFileToDatabase(FileInfo sourceFileInfo)
        {
            logger.Info($"Adding {sourceFileInfo.FullName} to database");

            var entryFilePath = GetEntryFilePath(sourceFileInfo.FullName);
            var fileInfo = new FileInfo(sourceFileInfo.FullName);
            var fileEntry = new FileEntry
            {
                Path = entryFilePath,
                Size = fileInfo.Length,
                FSDateCreated = fileInfo.CreationTimeUtc,
                FSDateModified = fileInfo.LastWriteTimeUtc,
                BackupLastUpdated = DateTimeOffset.Now
            };
            var entityEntry = context!.FileEntries.Add(fileEntry);

            modifiedRowCount += 1;
            if (modifiedRowCount % SaveChangesInterval == 0)
            {
                context.SaveChanges();
            }

            return entityEntry.Entity;
        }

        private void UpdateFileEntryTimestamps(FileEntry entry, DateTimeOffset newLastModifiedDate, DateTimeOffset newBackupDate)
        {
            entry.FSDateModified = newLastModifiedDate;
            entry.BackupLastUpdated = newBackupDate;

            modifiedRowCount += 1;
            if (modifiedRowCount % SaveChangesInterval == 0)
            {
                context!.SaveChanges();
            }
        }

        private void MarkFileAsDeletedFromSource(FileEntry entry)
        {
            entry.FileDeletedOnLastBackup = true;

            modifiedRowCount += 1;
            if (modifiedRowCount % SaveChangesInterval == 0)
            {
                context!.SaveChanges();
            }
        }

        private static string GetFileSizeString(long size)
        {
            var isNegative = size < 0;

            var sizeString = Math.Abs(size) switch
            {
                < 1024 => $"{size} bytes",
                < 1024 * 1024 => $"{size / 1024d:F2} KB",
                < 1024 * 1024 * 1024 => $"{size / 1024d / 1024d:F2} MB",
                _ => $"{size / 1024d / 1024d / 1024d:F2} GB",
            };

            return isNegative ? $"-{sizeString}" : sizeString;
        }

        public void Dispose()
        {
            context?.SaveChanges();
            context?.Dispose();
        }
    }
}
