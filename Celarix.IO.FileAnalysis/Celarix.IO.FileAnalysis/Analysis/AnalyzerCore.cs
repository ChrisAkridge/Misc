using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Celarix.IO.FileAnalysis.Utilities;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.Analysis
{
	public sealed class AnalyzerCore : JobPhase
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private IEnumerator<string> pathFileEnumerator;
        private AdvancedProgress advancedProgress;

        public override AnalysisPhase AnalysisPhase => AnalysisPhase.AnalyzingFiles;

        public override void StartOrResume(AnalysisJob job)
        {
            AnalysisJob = job;
            logger.Info("(START NEW PHASE) Beginning file analysis! This may take awhile.");

            var clientAnalyzer = new ClientAnalyzer();
            advancedProgress = new AdvancedProgress(AnalysisJob.EstimatedRemainingFiles, AnalysisJob.PhaseStartedOn)
            {
                CurrentAmount = AnalysisJob.EstimatedTotalFiles - AnalysisJob.EstimatedRemainingFiles
            };

            while (CreateEnumeratorForOutputFiles())
            {
                do
                {
                    var pathFilePath = pathFileEnumerator.Current;
                    var fileToAnalyzePath = LongFile.ReadAllText(pathFilePath);

                    if (string.IsNullOrEmpty(fileToAnalyzePath))
                    {
                        logger.Warn("Path file had no path in it.");
                        DeletePathFile(pathFilePath);
                        OnFileAnalyzed();
                        logger.Info($"An estimated {AnalysisJob.EstimatedRemainingFiles:N0} files remain.");
                        continue;
                    }

                    if (!LongFile.Exists(fileToAnalyzePath))
                    {
                        logger.Warn(
                            $"Could not find file {fileToAnalyzePath} for analysis; path file written for non-existent file!");
                        DeletePathFile(pathFilePath);
                        OnFileAnalyzed();
                        logger.Info($"An estimated {AnalysisJob.EstimatedRemainingFiles:N0} files remain.");
                        continue;
                    }
                    
                    logger.Trace($"Analyzing {fileToAnalyzePath}...");

                    var clientGeneratedFiles = clientAnalyzer.TryAnalyzeWithClients(fileToAnalyzePath);

                    if (clientGeneratedFiles.Any())
                    {
                        var addedFileCount = PathFileWriter.WritePathFiles(job, clientGeneratedFiles);
                        AnalysisJob.IncreaseEstimatedFileCount(addedFileCount);
                        advancedProgress.TotalAmount += addedFileCount;
                        AnalysisJob.SaveJobFile();
                        logger.Info(
                            $"Analysis added {addedFileCount:N0} additional files for a new total of {AnalysisJob.EstimatedRemainingFiles:#,###} files.");
                    }
                    else if (ImageIdentifier.IsValidImageFile(fileToAnalyzePath))
                    {
                        // Assume that any file that was decompressable, disassemblable
                        // or decompilable is not an image file. Fixes (?) an issue
                        // where we were accessing the file too soon and getting
                        // a "file in use" error.
                        PathFileWriter.WriteImagePathFile(job, fileToAnalyzePath);
                    }

                    if (Utilities.Utilities.IsTextFile(fileToAnalyzePath))
                    {
                        TextMapGenerator.GenerateMapForTextFile(fileToAnalyzePath);
                        CSharpSourceFileMemberPrinter.TryPrintSourceFileMembers(fileToAnalyzePath);
                    }

                    DeletePathFile(pathFilePath);
                    OnFileAnalyzed();
                    logger.Info($"An estimated {AnalysisJob.EstimatedRemainingFiles:N0} files remain. {advancedProgress}");
                } while (pathFileEnumerator.MoveNext());
            }

            logger.Info("Analysis complete! Copying all image paths into a single file.");
            PathFileWriter.WriteImagePathsToFile(job);
        }

        private void OnFileAnalyzed()
        {
            AnalysisJob.DecreaseEstimatedFileCount(1);
            advancedProgress.CurrentAmount += 1;
        }

        private bool CreateEnumeratorForOutputFiles()
        {
            pathFileEnumerator?.Dispose();
            
            var pathFilesFolder = AnalysisJob.ToAbsolutePath(FileLocation.Output, SharedConstants.PathFileFolderPath);
            var enumerator = LongDirectory.EnumerateFiles(pathFilesFolder, "*", SearchOption.AllDirectories).GetEnumerator();

            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                return false;
            }

            pathFileEnumerator = enumerator;
            return true;
        }

        private void DeletePathFile(string pathFilePath)
        {
            var parentFolder = LongPath.GetDirectoryName(pathFilePath);
            
            logger.Trace($"Deleting path file at {pathFilePath}");
            LongFile.Delete(pathFilePath);

            if (LongDirectory.GetFiles(parentFolder, "*", SearchOption.TopDirectoryOnly).Length == 0)
            {
                logger.Trace($"{parentFolder} is empty, deleting...");
                LongDirectory.Delete(parentFolder);
            }
        }
    }
}
