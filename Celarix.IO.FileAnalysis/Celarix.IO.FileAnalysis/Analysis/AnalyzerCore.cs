using System;
using System.Collections.Generic;
using System.IO;
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

        public override AnalysisPhase AnalysisPhase => AnalysisPhase.AnalyzingFiles;

        public override void StartOrResume(AnalysisJob job)
        {
            AnalysisJob = job;
            logger.Info("(START NEW PHASE) Beginning file analysis! This may take awhile.");

            var clientAnalyzer = new ClientAnalyzer();

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
                        AnalysisJob.EstimatedRemainingFiles = Math.Max(0, AnalysisJob.EstimatedRemainingFiles - 1);
                        logger.Info($"An estimated {AnalysisJob.EstimatedRemainingFiles:N0} files remain.");
                        continue;
                    }
                    
                    logger.Info($"Analyzing {fileToAnalyzePath}...");

                    var clientGeneratedFiles = clientAnalyzer.TryAnalyzeWithClients(fileToAnalyzePath);
                    
                    if (clientGeneratedFiles.Count > 0)
                    {
                        AnalysisJob.EstimatedRemainingFiles += clientGeneratedFiles.Count;
                        logger.Info($"Analysis added {clientGeneratedFiles.Count:N0} additional files for a new total of {AnalysisJob.EstimatedRemainingFiles:#,###} files.");
                    }
                    
                    PathFileWriter.WritePathFiles(job, clientGeneratedFiles);

                    if (ImageIdentifier.IsValidImageFile(fileToAnalyzePath))
                    {
                        PathFileWriter.WriteImagePathFile(job, fileToAnalyzePath);
                    }

                    BinaryDrawer.CreateBinaryImage(fileToAnalyzePath);

                    if (Utilities.Utilities.IsTextFile(fileToAnalyzePath))
                    {
                        TextMapGenerator.GenerateMapForTextFile(fileToAnalyzePath);
                        CSharpSourceFileMemberPrinter.TryPrintSourceFileMembers(fileToAnalyzePath);
                    }

                    DeletePathFile(pathFilePath);
                    AnalysisJob.EstimatedRemainingFiles = Math.Max(0, AnalysisJob.EstimatedRemainingFiles - 1);
                    logger.Info($"An estimated {AnalysisJob.EstimatedRemainingFiles:N0} files remain.");
                } while (pathFileEnumerator.MoveNext());
            }

            logger.Info("Analysis complete! Copying all image paths into a single file.");
            PathFileWriter.WriteImagePathsToFile(job);
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
            
            logger.Info($"Deleting path file at {pathFilePath}");
            LongFile.Delete(pathFilePath);

            if (LongDirectory.GetFiles(parentFolder, "*", SearchOption.TopDirectoryOnly).Length == 0)
            {
                logger.Info($"{parentFolder} is empty, deleting...");
                LongDirectory.Delete(parentFolder);
            }
        }
    }
}
