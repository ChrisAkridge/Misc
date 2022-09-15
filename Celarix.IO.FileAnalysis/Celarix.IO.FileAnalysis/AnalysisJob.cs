using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Analysis;
using Celarix.IO.FileAnalysis.Deduplication;
using Celarix.IO.FileAnalysis.FileCopying;
using Celarix.IO.FileAnalysis.StatisticCSVs;
using Celarix.IO.FileAnalysis.Utilities;
using NLog;
using LongFile = Pri.LongPath.File;
using LongPath = Pri.LongPath.Path;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis
{
    public sealed class AnalysisJob
    {
        private const string AnalysisJobPath = "job\\job.json";

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly (AnalysisPhase phase, Type phaseType)[] phases = new[]
        {
            (AnalysisPhase.WritingStatisticCSVs, typeof(StatisticCSVWriter)),
            (AnalysisPhase.CopyingFiles, typeof(FileCopier)),
            (AnalysisPhase.DeduplicatingFiles, typeof(Deduplicator)),
            (AnalysisPhase.GeneratingPathFiles, typeof(PathFileGenerator)),
            (AnalysisPhase.AnalyzingFiles, typeof(AnalyzerCore))
        };
        
        public DateTimeOffset JobStartedOn { get; set; }
        public DateTimeOffset? PhaseStartedOn { get; set; }
        public string InputFolderPath { get; set; }
        public string OutputFolderPath { get; set; }
        public int CurrentPhaseIndex { get; set; }

        public int EstimatedRemainingFiles { get; set; }
        public int EstimatedTotalFiles { get; set; }
        public int OriginalFileCount { get; set; }

        [JsonIgnore]
        public AnalysisPhase CurrentPhase
        {
            get => phases[CurrentPhaseIndex].phase;
            set
            {
                var phaseIndex = 0;
                while (phases[phaseIndex].phase != value) { phaseIndex++; }
                CurrentPhaseIndex = phaseIndex;
            }
        }

        public static AnalysisJob CreateOrLoad(string inputFolderPath, string outputFolderPath)
        {
            var jobAbsolutePath = LongPath.Combine(outputFolderPath, AnalysisJobPath);

            if (!LongFile.Exists(jobAbsolutePath))
            {
                return CreateNewJob(inputFolderPath, outputFolderPath);
            }

            var jobJson = LongFile.ReadAllText(jobAbsolutePath);
            return JsonSerializer.Deserialize<AnalysisJob>(jobJson);
        }
        
        public static AnalysisJob CreateNewJob(string inputFolderPath, string outputFolderPath)
        {
            var job = new AnalysisJob
            {
                InputFolderPath = inputFolderPath,
                OutputFolderPath = outputFolderPath,
                JobStartedOn = DateTimeOffset.Now
            };

            job.SaveJobFile();

            return job;
        }

        public void StartOrResume()
        {
            LoggingConfigurer.ConfigureLogging(OutputFolderPath);

            logger.Info("Building character cache...");
            CharacterCache.Build();

            while (CurrentPhaseIndex < phases.Length)
            {
                logger.Info($"Beginning phase {CurrentPhase}; saving job to disk...");
                SaveJobFile();

                var phaseType = phases[CurrentPhaseIndex].phaseType;
                var phaseInstance = (JobPhase)Activator.CreateInstance(phaseType);
                PhaseStartedOn ??= DateTimeOffset.Now;
                phaseInstance.StartOrResume(this);

                PhaseStartedOn = null;
                logger.Info($"Finished phase {CurrentPhase}");
                CurrentPhaseIndex += 1;
            }

            logger.Info("Analysis job complete!");
        }

        public string ToAbsolutePath(FileLocation location, string relativePath) =>
            LongPath.Combine((location == FileLocation.Input) ? InputFolderPath : OutputFolderPath, relativePath);

        public void IncreaseEstimatedFileCount(int filesFound)
        {
            EstimatedRemainingFiles += filesFound;
            EstimatedTotalFiles += filesFound;
            SaveJobFile();
        }

        public void DecreaseEstimatedFileCount(int filesProcessed)
        {
            EstimatedRemainingFiles -= filesProcessed;
            if (EstimatedRemainingFiles < 0)
            {
                EstimatedRemainingFiles = 0;
            }
            SaveJobFile();
        }
        
        public void SaveJobFile()
        {
            var jobJson = JsonSerializer.Serialize(this);
            var jobAbsolutePath = LongPath.Combine(OutputFolderPath, AnalysisJobPath);
            var jobFolderInfo = new LongDirectoryInfo(LongPath.GetDirectoryName(jobAbsolutePath));
            jobFolderInfo.Create();
            
            LongFile.WriteAllText(jobAbsolutePath, jobJson);
        }
    }
}
