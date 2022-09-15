using System;
using Celarix.IO.FileAnalysis.Analysis.Models;
using Celarix.IO.FileAnalysis.Extensions;
using NLog;
using SevenZip;
using LongPath = Pri.LongPath.Path;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.Analysis.Clients
{
    internal static class SevenZipClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static ExtractResult TryExtract(string filePath)
        {
            try
            {
                logger.Trace($"Attempting to extract {filePath}...");
                
                var outputFolder = GetExtractionPathForFile(filePath);

                if (LongDirectory.Exists(outputFolder))
                {
                    logger.Warn($"The archive at {filePath} was already extracted! Skipping...");
                    return new ExtractResult(true, 0);
                }
                
                using var reader = new SevenZipExtractor(filePath);

                reader.ExtractArchive(outputFolder);
                logger.Trace($"Extracted {reader.FilesCount} file(s) from {filePath}");
                return new ExtractResult(true, (int)reader.FilesCount);
            }
            catch (Exception ex)
            {
                logger.Trace($"Failed to extract {filePath} ({ex.Message})");

                return new ExtractResult(false, 0);
            }
        }

        public static bool IsArchive(string filePath)
        {
            try
            {
                using var reader = new SevenZipExtractor(filePath);

                var isArchive = reader.FilesCount > 0;
                logger.Trace($"{filePath} {(isArchive ? "is" : "is not")} an archive");

                return isArchive;
            }
            catch (Exception)
            {
                logger.Trace($"{filePath} is not an archive.");
                return false;
            }
        }

        public static string GetExtractionPathForFile(string filePath) =>
            LongPath.Combine(LongPath.GetDirectoryName(filePath),
                LongPath.GetFileNameWithoutExtension(filePath) + SharedConstants.ExtractedFileFolderSuffix);
    }
}
