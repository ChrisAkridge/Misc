using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.Imaging.ZoomableCanvas;
using Celarix.IO.FileAnalysis.Analysis;
using SixLabors.ImageSharp;
using LongFile = Pri.LongPath.File;
using LongPath = Pri.LongPath.Path;
using LongDirectory = Pri.LongPath.Directory;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis.Utilities
{
    public static class Utilities
    {
        public static FileStream OpenWriteCreatePath(string filePath)
        {
            var parentFolderPath = LongPath.GetDirectoryName(filePath);
            var parentFolderInfo = new LongDirectoryInfo(parentFolderPath);
            parentFolderInfo.Create();

            return LongFile.OpenWrite(filePath);
        }

        public static string GetTextFilePagesFolderPath(string filePath) =>
            LongPath.Combine(LongPath.GetDirectoryName(filePath),
                LongPath.GetFileNameWithoutExtension(filePath) + "_ext", SharedConstants.TextFileFolderPath);

        public static void DrawZoomLevelsForLevel0CanvasTiles(string level0FolderPath)
        {
            var currentZoomLevelFolderPath = level0FolderPath;
            var canvasFolderPath = LongPath.GetDirectoryName(level0FolderPath);
            var nextZoomLevel = 1;

            while (ZoomLevelGenerator.TryCombineImagesForNextZoomLevel(currentZoomLevelFolderPath,
                canvasFolderPath,
                nextZoomLevel,
                new StringProgressToLogger()))
            {
                nextZoomLevel += 1;

                currentZoomLevelFolderPath = LongPath.Combine(canvasFolderPath,
                    (nextZoomLevel - 1).ToString());
            }
        }

        public static bool IsTextFile(string filePath)
        {
            using var stream = LongFile.OpenRead(filePath);
            var buffer = new byte[4096];
            int readBytes = stream.Read(buffer, 0, 4096);
            
            // Text file heuristic: a file will be treated as text if the first
            // 4,096 bytes fit the following criteria:
            //  - No two consecutive 0x00 bytes

            return !HasConsecutiveZeroBytes(buffer, readBytes);
        }

        public static bool TryShortenFilePath(string text, out string result)
        {
            char pathSeparator = Path.DirectorySeparatorChar;
            var parts = text.Split(pathSeparator).ToList();

            string driveLetter = parts[0];
            parts.RemoveAt(0);

            string fileName = parts[^1];
            parts.RemoveAt(parts.Count - 1);

            string longestPart = parts.OrderByDescending(p => p.Length).First();

            if (longestPart.Length == 3)
            {
                result = text;

                return false;
            }

            int longestPartIndex = parts.IndexOf(longestPart);
            parts[longestPartIndex] = "...";

            result = string.Concat(driveLetter, pathSeparator, string.Join(pathSeparator.ToString(), parts),
                pathSeparator, fileName);

            return true;
        }

        private static bool HasConsecutiveZeroBytes(byte[] buffer, int readBytes)
        {
            byte? lastByte = null;

            for (var i = 0; i < readBytes; i++)
            {
                var b = buffer[i];

                if (b == 0 && lastByte == 0) { return true; }

                lastByte = b;
            }

            return false;
        }
    }
}
