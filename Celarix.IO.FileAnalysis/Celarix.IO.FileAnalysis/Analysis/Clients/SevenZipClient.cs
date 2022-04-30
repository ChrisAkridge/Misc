using System;
using Celarix.IO.FileAnalysis.Analysis.Models;
using Celarix.IO.FileAnalysis.Extensions;
using NLog;
using SevenZip;
using LongPath = Pri.LongPath.Path;

namespace Celarix.IO.FileAnalysis.Analysis.Clients
{
    internal static class SevenZipClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static ExtractResult TryExtract(string filePath)
        {
            try
            {
                logger.Info($"Attempting to extract {filePath}...");
                
                var outputFolder = GetExtractionPathForFile(filePath);
                using var reader = new SevenZipExtractor(filePath);

                reader.ExtractArchive(outputFolder);
                logger.Info($"Extracted {reader.FilesCount} file(s) from {filePath}");
                return new ExtractResult(true, (int)reader.FilesCount);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);

                return new ExtractResult(false, 0);
            }
        }

        public static bool IsArchive(string filePath)
        {
            try
            {
                using var reader = new SevenZipExtractor(filePath);

                var isArchive = reader.FilesCount > 0;
                logger.Info($"{filePath} {(isArchive ? "is" : "is not")} an archive");

                return isArchive;
            }
            catch (Exception)
            {
                logger.Info($"{filePath} is not an archive.");
                return false;
            }
        }

        public static string GetExtractionPathForFile(string filePath) =>
            LongPath.Combine(LongPath.GetDirectoryName(filePath),
                LongPath.GetFileNameWithoutExtension(filePath) + SharedConstants.ExtractedFileFolderSuffix);
    }
}
