using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Extensions;
using NLog;
using LongFile = Pri.LongPath.File;
using LongPath = Pri.LongPath.Path;
using LongDirectory = Pri.LongPath.Directory;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis.FileCopying
{
    public sealed class FileCopier : JobPhase, IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly SHA256 sha256Hasher = SHA256.Create();
        
        public override AnalysisPhase AnalysisPhase => AnalysisPhase.CopyingFiles;

        public override void StartOrResume(AnalysisJob job)
        {
            AnalysisJob = job;
            logger.Info("(START NEW PHASE) Copying files from input...");
            logger.Info("Checking for existing hash files...");

            var hashFilePath = job.ToAbsolutePath(FileLocation.Output, SharedConstants.HashFilePath);

            if (LongFile.Exists(hashFilePath))
            {
                logger.Info("Found hash file, deleting...");
                LongFile.Delete(hashFilePath);
            }
            
            WriteFileList();
            WriteFileHashes();
            CopyFiles();
        }

        private void WriteFileList()
        {
            logger.Info("Writing list of files to copy...");
            var currentFileListIndex = 0;
            var filesWrittenToList = 0;
            StreamWriter fileListWriter = OpenFileListWriter(currentFileListIndex);

            foreach (var filePath in LongDirectory.EnumerateFiles(AnalysisJob.InputFolderPath, "*", SearchOption.AllDirectories))
            {
                var outputPath = GetDestinationPath(filePath);
                fileListWriter.WriteLine(outputPath);
                filesWrittenToList += 1;

                if (filesWrittenToList == SharedConstants.DefaultBufferCapacity)
                {
                    logger.Info($"Wrote {(currentFileListIndex + 1) * SharedConstants.DefaultBufferCapacity} files to file lists");
                    fileListWriter.Dispose();

                    currentFileListIndex += 1;
                    fileListWriter = OpenFileListWriter(currentFileListIndex);
                }
            }
            
            fileListWriter.Dispose();
            logger.Info($"Wrote {(currentFileListIndex * SharedConstants.DefaultBufferCapacity) + filesWrittenToList} files");
        }

        private StreamWriter OpenFileListWriter(int fileListIndex)
        {
            var fileListFolderPath = AnalysisJob.ToAbsolutePath(FileLocation.Output, SharedConstants.ListFileFolderPath);
            new LongDirectoryInfo(fileListFolderPath).Create();

            return new StreamWriter(Utilities.Utilities.OpenWriteCreatePath(
                LongPath.Combine(fileListFolderPath,
                    $"files_{fileListIndex}.txt")));
        }

        private void WriteFileHashes()
        {
            logger.Info($"Writing SHA-256 hashes for all files...");
            
            var fileListPaths = LongDirectory
                .EnumerateFiles(AnalysisJob.ToAbsolutePath(FileLocation.Output,
                    SharedConstants.ListFileFolderPath), "*.txt", SearchOption.TopDirectoryOnly)
                .OrderBy(p => p);
            using var hashWriter = new StreamWriter(Utilities.Utilities.OpenWriteCreatePath(
                AnalysisJob.ToAbsolutePath(FileLocation.Output, SharedConstants.HashFilePath)));
            var hashesWritten = 0;

            foreach (var fileListPath in fileListPaths)
            {
                logger.Info($"Writing hashes for files listed in {fileListPath}...");
                var filePathsInList = LongFile.ReadAllLines(fileListPath);

                foreach (var outputFilePath in filePathsInList)
                {
                    var inputFilePath = GetSourcePath(outputFilePath);
                    logger.Info($"Writing hash for {inputFilePath}");
                    var fileStream = LongFile.OpenRead(inputFilePath);
                    fileStream.Position = 0L;
                    var hash = string.Concat(sha256Hasher
                        .ComputeHash(fileStream)
                        .Select(b => b.ToString("X2")));
                    
                    hashWriter.WriteLine($"{hash} : {outputFilePath}");

                    hashesWritten += 1;
                    if (hashesWritten == SharedConstants.DefaultBufferCapacity)
                    {
                        hashWriter.Flush();
                    }
                }
            }
        }

        private void CopyFiles()
        {
            logger.Info("Copying files from input to output...");
            
            var fileListPaths = LongDirectory
                .EnumerateFiles(AnalysisJob.ToAbsolutePath(FileLocation.Output,
                    SharedConstants.ListFileFolderPath), "*.txt", SearchOption.TopDirectoryOnly)
                .OrderBy(p => p);

            foreach (var fileListPath in fileListPaths)
            {
                logger.Info($"Copying files listed in {fileListPath}");
                var filePathsInList = LongFile.ReadAllLines(fileListPath);

                foreach (var inputFilePath in filePathsInList.Select(f => GetSourcePath(f)))
                {
                    logger.Info($"Copying {inputFilePath}");

                    try
                    {
                        CopyFileIfNotPresent(inputFilePath);
                        AnalysisJob.EstimatedRemainingFiles += 1;
                    }
                    catch (Exception ex)
                    {
                        logger.LogException(ex);
                    }
                }
            }
        }

        private void CopyFileIfNotPresent(string filePathFromInput)
        {
            var destinationPath = GetDestinationPath(filePathFromInput);

            if (LongFile.Exists(destinationPath))
            {
                return;
            }
            
            var destinationFolder = LongPath.GetDirectoryName(destinationPath);
            new LongDirectoryInfo(destinationFolder).Create();
                
            LongFile.Copy(filePathFromInput, destinationPath);
        }

        private string GetDestinationPath(string filePathFromInput)
        {
            var inputFolderLength = AnalysisJob.InputFolderPath.Length + 1;
            var relativeFilePath = filePathFromInput.Substring(inputFolderLength);

            return AnalysisJob.ToAbsolutePath(FileLocation.Output, LongPath.Combine(
                SharedConstants.OutputFileFolderPath,
                relativeFilePath));
        }

        private string GetSourcePath(string filePathFromOutput)
        {
            var outputFolderLength = AnalysisJob.OutputFolderPath.Length + SharedConstants.OutputFileFolderPath.Length + 2;
            var relativeFilePath = filePathFromOutput.Substring(outputFolderLength);

            return AnalysisJob.ToAbsolutePath(FileLocation.Input, relativeFilePath);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            sha256Hasher?.Dispose();
        }
    }
}
