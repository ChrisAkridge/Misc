using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Utilities;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis.Analysis
{
    public sealed class PathFileGenerator : JobPhase
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private AdvancedProgress advancedProgress;
        
        public override AnalysisPhase AnalysisPhase => AnalysisPhase.GeneratingPathFiles;

        public override void StartOrResume(AnalysisJob job)
        {
            AnalysisJob = job;
            advancedProgress = new AdvancedProgress(AnalysisJob.OriginalFileCount, AnalysisJob.PhaseStartedOn);
            
            logger.Info("(START NEW PHASE) Generating path files for output files...");

            foreach (var outputFilePath in LongDirectory.EnumerateFiles(
                job.ToAbsolutePath(FileLocation.Output, SharedConstants.OutputFileFolderPath), "*",
                SearchOption.AllDirectories))
            {
                logger.Info($"Writing path file for {outputFilePath}");
                PathFileWriter.WritePathFile(job, outputFilePath);
                advancedProgress.CurrentAmount += 1;
                logger.Info(advancedProgress.ToString());
            }
        }
    }
}
