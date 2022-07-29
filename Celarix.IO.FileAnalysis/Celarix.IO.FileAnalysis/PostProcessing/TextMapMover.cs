﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    internal static class TextMapMover
    {
        private const string TextMapPath = "textMaps";
        private const string DefaultTextMapPath = "default";
        private const string AssemblyFileTextMapPath = "asm";
        private const string CSharpSourceFileTextMapPath = "cs";
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void MoveAllTextMaps(string folderPath)
        {
            logger.Info($"Moving all text maps from {folderPath}...");

            var textMaps = FindAllTextMapsInFolder(folderPath);
            logger.Info($"Found {textMaps.Count:N0} text maps in {folderPath}.");

            LongDirectory.CreateDirectory(LongPath.Combine(folderPath, TextMapPath));
            LongDirectory.CreateDirectory(LongPath.Combine(folderPath, TextMapPath, DefaultTextMapPath));
            LongDirectory.CreateDirectory(LongPath.Combine(folderPath, TextMapPath, AssemblyFileTextMapPath));
            LongDirectory.CreateDirectory(LongPath.Combine(folderPath, TextMapPath, CSharpSourceFileTextMapPath));

            foreach (var (filePath, kind) in textMaps)
            {
                var destinationFolderPath = kind switch
                {
                    TextMapKind.Default => LongPath.Combine(folderPath, TextMapPath, DefaultTextMapPath),
                    TextMapKind.AssemblyFile => LongPath.Combine(folderPath, TextMapPath, AssemblyFileTextMapPath),
                    TextMapKind.CSharpSourceFile => LongPath.Combine(folderPath, TextMapPath, CSharpSourceFileTextMapPath),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var destinationFileName = $"{Guid.NewGuid()}.png";
                var destinationPath = LongPath.Combine(destinationFolderPath, destinationFileName);
                
                LongFile.Move(filePath, destinationPath);
                logger.Info($"Moved {filePath} to {destinationPath}");
            }
            
            logger.Info("Moved all text maps.");
        }

        private static List<(string filePath, TextMapKind kind)> FindAllTextMapsInFolder(string folderPath)
        {
            var textMaps = new List<(string filePath, TextMapKind kind)>();

            var filesInFolder = LongDirectory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            var foldersInFolder = LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);

            textMaps.AddRange(filesInFolder
                .Where(f => LongPath.GetFileName(f).Equals("textMap.png", StringComparison.InvariantCultureIgnoreCase))
                .Select(f => (f, DetermineTextMapKind(f))));

            foreach (var childFolderPath in foldersInFolder)
            {
                textMaps.AddRange(FindAllTextMapsInFolder(childFolderPath));
            }

            logger.Info($"Folder {folderPath} has {textMaps.Count:N0} text maps.");
            return textMaps;
        }

        private static TextMapKind DetermineTextMapKind(string textMapPath)
        {
            if (textMapPath.Contains($"{LongPath.DirectorySeparatorChar}disasm"))
            {
                return TextMapKind.AssemblyFile;
            }

            var extractionDirectory = LongDirectory.GetParent(textMapPath);
            var originalFileNameNoExtension = LongPath.GetFileNameWithoutExtension(extractionDirectory.FullName.Substring(0, extractionDirectory.FullName.Length - 4));
            var originalFileDirectory = LongDirectory.GetParent(extractionDirectory.FullName);
            var matchingFiles =
                originalFileDirectory.GetFiles($"{originalFileNameNoExtension}*", SearchOption.TopDirectoryOnly);

            return matchingFiles.Any(f =>
                f.Name.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase)
                || f.Name.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase))
                ? TextMapKind.CSharpSourceFile
                : TextMapKind.Default;
        }
    }
}
