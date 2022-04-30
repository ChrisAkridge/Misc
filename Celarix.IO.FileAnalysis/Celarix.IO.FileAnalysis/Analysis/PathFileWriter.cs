﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis.Analysis
{
    internal static class PathFileWriter
    {
        private static readonly byte[] guidBytes = new byte[16];
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private static string GetPathFilePath(AnalysisJob job, Guid guid)
        {
            guid.TryWriteBytes(guidBytes);

            return job.ToAbsolutePath(FileLocation.Output,
                LongPath.Combine(LongPath.Combine(SharedConstants.PathFileFolderPath,
                        guidBytes[0].ToString("X2"),
                        guidBytes[1].ToString("X2"),
                        guidBytes[2].ToString("X2")),
                    $"{guid}.txt"));
        }

        private static string GetImagePathFilePath(AnalysisJob job, Guid guid)
        {
            guid.TryWriteBytes(guidBytes);

            return job.ToAbsolutePath(FileLocation.Output,
                LongPath.Combine(LongPath.Combine(SharedConstants.ImagePathFileFolderPath,
                        guidBytes[0].ToString("X2"),
                        guidBytes[1].ToString("X2"),
                        guidBytes[2].ToString("X2")),
                    $"{guid}.txt"));
        }

        public static void WritePathFile(AnalysisJob job, string filePath)
        {
            if (filePath.Contains("\\bytes\\") || filePath.Contains("textMap") || filePath.Contains("bytes.png"))
            {
                logger.Warn(
                    $"Attempted to add a binary drawing file or text map to the list of files. At {filePath}");
                return;

                //logger.Fatal($"A binary drawing file or text map has been added to the list of files to process! At {filePath}");
                //logger.Fatal(Environment.StackTrace);
                //Environment.Exit(-1);
            }
            
            var pathFilePath = GetPathFilePath(job, Guid.NewGuid());
            var pathFileFolderInfo = new LongDirectoryInfo(LongPath.GetDirectoryName(pathFilePath));
            pathFileFolderInfo.Create();
            LongFile.WriteAllText(pathFilePath, filePath);
        }

        public static void WriteImagePathFile(AnalysisJob job, string imageFilePath)
        {
            var imagePathFilePath = GetImagePathFilePath(job, Guid.NewGuid());
            var pathFileFolderInfo = new LongDirectoryInfo(LongPath.GetDirectoryName(imagePathFilePath));
            pathFileFolderInfo.Create();
            LongFile.WriteAllText(imagePathFilePath, imageFilePath);
        }

        public static void WritePathFiles(AnalysisJob job, IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                WritePathFile(job, filePath);
            }
        }

        public static void WriteImagePathsToFile(AnalysisJob job)
        {
            StreamWriter imagePathWriter;

            try
            {
                imagePathWriter = new StreamWriter(LongFile.OpenWrite(job.ToAbsolutePath(FileLocation.Output,
                    SharedConstants.ImagePathsFilePath)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            var imagePathFileFolderPath =
                job.ToAbsolutePath(FileLocation.Output, SharedConstants.ImagePathFileFolderPath);

            foreach (var imagePathFilePath in LongDirectory.EnumerateFiles(imagePathFileFolderPath, "*.txt", SearchOption.AllDirectories))
            {
                logger.Info($"Copying image path {{{LongPath.GetFileNameWithoutExtension(imagePathFilePath)}}}");
                var imagePath = LongFile.ReadAllText(imagePathFilePath);
                imagePathWriter.WriteLine(imagePath);
            }

            imagePathWriter.Dispose();
        }
    }
}