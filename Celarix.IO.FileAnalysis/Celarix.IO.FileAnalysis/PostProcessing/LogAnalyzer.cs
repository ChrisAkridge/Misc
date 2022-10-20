using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Utilities;
using NLog;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public static class LogAnalyzer
    {
        private const int TimestampPart = 0;
        private const int LogLevelPart = 1;
        private const int MessagePart = 2;
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static AdvancedProgress advancedProgress;
        
        // Log file name format:
        // YYYY-MM-DD_.#.log
        // # starts at 1 and increases (no zero padding)
        // File with date followed by _ is first file in folder for date
        // File with just date is always last file in folder for date
        private sealed class LogFilePathComparer : IComparer<string>
        {
            /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.
            /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description><paramref name="x" /> is less than <paramref name="y" />.</description></item><item><term> Zero</term><description><paramref name="x" /> equals <paramref name="y" />.</description></item><item><term> Greater than zero</term><description><paramref name="x" /> is greater than <paramref name="y" />.</description></item></list></returns>
            public int Compare(string x, string y)
            {
                var xFileName = LongPath.GetFileNameWithoutExtension(x);
                var yFileName = LongPath.GetFileNameWithoutExtension(y);

                var (xDate, xNumber) = GetFileDateAndNumber(xFileName);
                var (yDate, yNumber) = GetFileDateAndNumber(yFileName);

                return xDate == yDate
                    ? xNumber.CompareTo(yNumber)
                    : xDate.CompareTo(yDate);
            }

            private static (DateTimeOffset date, int number) GetFileDateAndNumber(string fileName)
            {
                var parts = fileName.Split('_');
                
                if (parts.Length == 2)
                {
                    return string.IsNullOrEmpty(parts[1])
                        ? (DateTimeOffset.Parse(parts[0]), int.MinValue)
                        : (DateTimeOffset.Parse(parts[0]), int.Parse(parts[1][1..]));
                }
                
                return (DateTimeOffset.Parse(parts[0]), int.MaxValue);
            }
        }

        private enum MessageType
        {
            StartPhase,
            CompletePhase,
            WritingFileStatistics,
            CountingChildFolders,
            WritingFileHash,
            CopyingFile,
            DuplicateFound,
            WritingPathFile,
            EstimatedFileCount,
            Other
        }

        public static void AnalyzeLogsToCSVs(string folderPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Analyzing logs in {folderPath}...");

            var logFilePaths = LongDirectory
                .GetFiles(folderPath, "*.log", SearchOption.TopDirectoryOnly)
                .OrderBy(p => p, new LogFilePathComparer())
                .ToArray();
            
            logger.Info($"Found {logFilePaths.Length:N0} logs in {folderPath}");
            advancedProgress = new AdvancedProgress(logFilePaths.Length, DateTimeOffset.Now);
            
            LongDirectory.CreateDirectory(LongPath.Combine(folderPath, "stats"));
            var writtenCSVFiles = 0;
            var fileCountCSVPath = LongPath.Combine(folderPath, "stats", "fileCount_0.csv");
            var fileCountCSVStream = new StreamWriter(LongFile.OpenWrite(fileCountCSVPath));
            fileCountCSVStream.WriteLine("timestamp,remaining");
            var writtenCSVLines = 1;

            var logLines = GetAllLogLines(logFilePaths);
            var lineCount = 0L;
            var allLineLength = 0L;
            var messageTypeCounts = new Dictionary<MessageType, long>
            {
                { MessageType.StartPhase, 0L},
                { MessageType.CompletePhase, 0L},
                { MessageType.WritingFileStatistics, 0L},
                { MessageType.CountingChildFolders, 0L},
                { MessageType.WritingFileHash, 0L},
                { MessageType.CopyingFile, 0L},
                { MessageType.DuplicateFound, 0L},
                { MessageType.WritingPathFile, 0L},
                { MessageType.EstimatedFileCount, 0L},
                { MessageType.Other, 0L},
            };
            var timingBucketIntervals = new[]
            {
                1,
                5,
                1 * 10,
                5 * 10,
                10 * 10,
                50 * 10,
                100 * 10,
                500 * 10,
                1 * 1000 * 10,
                5 * 1000 * 10,
                10 * 1000 * 10,
                30 * 1000 * 10,
                60 * 1000 * 10,
                5 * 60 * 1000 * 10,
                10 * 60 * 1000 * 10,
                30 * 60 * 1000 * 10,
                60 * 60 * 1000 * 10,
                int.MaxValue
            };
            var timingBuckets = new Dictionary<int, long>
            {
                { 1, 0L }, // 100 μs
                { 5, 0L }, // 500 μs
                { 1 * 10, 0L }, // 1 ms
                { 5 * 10, 0L }, // 5 ms
                { 10 * 10, 0L }, // 10 ms
                { 50 * 10, 0L }, // 50 ms
                { 100 * 10, 0L }, // 100 ms
                { 500 * 10, 0L }, // 500 ms
                { 1000 * 10, 0L }, // 1 sec
                { 5 * 1000 * 10, 0L }, // 5 sec
                { 10 * 1000 * 10, 0L }, // 10 sec
                { 30 * 1000 * 10, 0L }, // 30 sec
                { 60 * 1000 * 10, 0L }, // 1 min
                { 5 * 60 * 1000 * 10, 0L }, // 5 min
                { 10 * 60 * 1000 * 10, 0L }, // 10 min
                { 30 * 60 * 1000 * 10, 0L }, // 30 min
                { 60 * 60 * 1000 * 10, 0L }, // 60 min
                { int.MaxValue, 0L } // Longer
            };

            DateTimeOffset? firstTimeStamp = null;
            DateTimeOffset? lastTimestamp = null;
            foreach (var line in logLines)
            {
                lineCount += 1;
                if (lineCount % 10000 == 0)
                {
                    logger.Info($"Processed {lineCount:N0} lines");
                }

                allLineLength += line.Length;
                
                var lineParts = line.Split('|', 3);

                if (lineParts.Length != 3)
                {
                    continue;
                }
                
                var timeStamp = DateTimeOffset.Parse(lineParts[TimestampPart]);

                var message = lineParts[MessagePart];
                var messageType = IdentifyMessage(message);
                messageTypeCounts[messageType] += 1;

                if (lastTimestamp == null)
                {
                    firstTimeStamp = timeStamp;
                    lastTimestamp = timeStamp;
                }
                else
                {
                    var delta = timeStamp - lastTimestamp;
                    for (var i = 0; i < timingBucketIntervals.Length; i++)
                    {
                        var interval = timingBucketIntervals[i];
                        var intervalSpan = TimeSpan.FromMilliseconds(0.1d) * interval;
                        if (delta < intervalSpan)
                        {
                            timingBuckets[interval] += 1;
                            break;
                        }
                    }
                    lastTimestamp = timeStamp;
                }

                if (messageType == MessageType.EstimatedFileCount)
                {
                    fileCountCSVStream.WriteLine($"{timeStamp:yyyy_MM_dd}T{timeStamp:HH:mm:ss.ffff},{GetEstimateFileCountFromMessage(message)}");
                    writtenCSVLines += 1;
                    if (writtenCSVLines == 1048576)
                    {
                        writtenCSVFiles += 1;
                        fileCountCSVStream.Close();
                        fileCountCSVStream.Dispose();

                        fileCountCSVPath = LongPath.Combine(folderPath, "stats", $"fileCount_{writtenCSVFiles}.csv");
                        fileCountCSVStream = new StreamWriter(LongFile.OpenWrite(fileCountCSVPath));
                        fileCountCSVStream.WriteLine("timestamp,remaining");
                        writtenCSVLines = 1;
                    }
                }
            }
            
            fileCountCSVStream.Close();
            fileCountCSVStream.Dispose();

            var finalStatsBuilder = new StringBuilder();
            finalStatsBuilder.AppendLine("interval,count");
            foreach (var bucketKvp in timingBuckets.OrderBy(kvp => kvp.Key))
            {
                finalStatsBuilder.AppendLine($"{TimeSpan.FromMilliseconds(0.1d * bucketKvp.Key)},{bucketKvp.Value}");
            }
            finalStatsBuilder.AppendLine();
            finalStatsBuilder.AppendLine("messageType,count");
            foreach (var kvp in messageTypeCounts)
            {
                finalStatsBuilder.AppendLine($"{kvp.Key},{kvp.Value}");
            }
            finalStatsBuilder.AppendLine();
            finalStatsBuilder.AppendLine($"totalLines,{lineCount}");
            finalStatsBuilder.AppendLine($"totalLineLength,{allLineLength}");
            finalStatsBuilder.AppendLine($"firstMessage,{firstTimeStamp}");
            finalStatsBuilder.AppendLine($"lastMessage,{lastTimestamp}");
            
            LongFile.WriteAllText(LongPath.Combine(folderPath, "stats", "finalStats.csv"),
                finalStatsBuilder.ToString());
            
            logger.Info($"Completed log analysis of {folderPath}");
        }

        private static IEnumerable<string> GetAllLogLines(IEnumerable<string> logFilePaths)
        {
            foreach (var reader in logFilePaths.Select(p => new StreamReader(p)))
            {
                while (reader.ReadLine() is { } currentLine)
                {
                    yield return currentLine;
                }
                reader.Close();
                reader.Dispose();

                advancedProgress.CurrentAmount += 1;
                logger.Info(advancedProgress.ToString());
            }
        }

        private static MessageType IdentifyMessage(string message)
        {
            const StringComparison comparison = StringComparison.Ordinal;
            if (message.StartsWith("(START NEW PHASE)", comparison))
            {
                return MessageType.StartPhase;
            }

            if (message.StartsWith("(COMPLETE PHASE)", comparison))
            {
                return MessageType.CompletePhase;
            }
            
            if (message.StartsWith("Writing stats for", comparison))
            {
                return MessageType.WritingFileStatistics;
            }
            
            if (message.EndsWith("child folders", comparison))
            {
                return MessageType.CountingChildFolders;
            }

            if (message.StartsWith("Writing hash for", comparison))
            {
                return MessageType.WritingFileHash;
            }

            if (message.StartsWith("Copying", comparison)
                && !message.StartsWith("Copying files ", comparison))
            {
                return MessageType.CopyingFile;
            }

            if (message.Contains("is a duplicate of", comparison))
            {
                return MessageType.DuplicateFound;
            }

            if (message.StartsWith("Writing path file for", comparison))
            {
                return MessageType.WritingPathFile;
            }

            if (message.Contains("estimate", comparison))
            {
                return MessageType.EstimatedFileCount;
            }

            return MessageType.Other;
        }

        private static int GetEstimateFileCountFromMessage(string message) =>
            int.Parse(message.Split(' ')[2], NumberStyles.AllowThousands);
    }
}
