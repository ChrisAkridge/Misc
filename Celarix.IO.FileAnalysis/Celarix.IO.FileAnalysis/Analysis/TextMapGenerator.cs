using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Extensions;
using Celarix.IO.FileAnalysis.Utilities;
using MoreLinq;
using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using LongFile = Pri.LongPath.File;
using LongPath = Pri.LongPath.Path;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis.Analysis
{
	public static class TextMapGenerator
    {
        private const double DefaultBackgroundSaturation = 0.15d;
        private const string TextMapPath = "textMap.png";
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void GenerateMapForTextFile(string filePath)
        {
            logger.Trace($"Writing text map for {filePath}...");

            var mapSavePath = GetSavePath(filePath);

            if (LongFile.Exists(mapSavePath))
            {
                logger.Warn($"The text map for the file at {filePath} already exists! Skipping...");
                return;
            }

            using var stream = new StreamReader(LongFile.OpenRead(filePath));
            var lineMaps = new List<BitArray>();
            var totalImagePixels = 0L;
            string currentLine;

			while ((currentLine = stream.ReadLine()) != null)
            {
                totalImagePixels += currentLine.Length;

                var lineMap = new BitArray(currentLine.Length);
                for (var i = 0; i < currentLine.Length; i++)
                {
                    lineMap[i] = !char.IsWhiteSpace(currentLine[i]);
                }
                lineMaps.Add(lineMap);
            }

            if (!lineMaps.Any() || totalImagePixels == 0)
            {
                logger.Trace("File was empty");
                return;
            }

            logger.Trace($"File has {lineMaps.Count} lines");

            var hueStep = 360d / lineMaps.Count;
            var linesPerColumn = (int)Math.Min(Math.Floor(Math.Sqrt(totalImagePixels)), lineMaps.Count);
            var columnMaps = lineMaps
                .Batch(linesPerColumn)
                .Select(c => c.ToArray())
                .ToArray();
            var columnWidths = columnMaps.Select(c => c.Max(l => l.Length)).ToArray();
            var totalImageWidth = columnWidths.Sum();
            var currentLineHue = 0d;

            if (totalImageWidth > 16384)
            {
                logger.Trace($"Whoa! This text map is {totalImageWidth} pixels wide! That's too big.");
                return;
            }

            const int headerHeight = 24;
            var resultImage = new Image<Rgb24>(totalImageWidth, linesPerColumn + headerHeight);
            var columnDrawX = 0;
            
            var shortenedPath = filePath;
            var font = SystemFonts.CreateFont("Consolas", 14f);
            
            while (TextMeasurer.Measure(shortenedPath, new RendererOptions(font)).Width > resultImage.Width)
            {
                if (!Utilities.Utilities.TryShortenFilePath(shortenedPath, out shortenedPath)) { break; }
            }

            CharacterCache.DrawString(resultImage, shortenedPath, Point.Empty);

            for (var i = 0; i < columnMaps.Length; i++)
            {
                var columnMap = columnMaps[i];
                var columnWidth = columnWidths[i];
                logger.Trace($"Drawing column {i + 1}...");
                
                if (columnWidth > 20000)
                {
                    logger.Trace(
                        "Whoa! This file has some really long lines! This is likely a minified file, skipping...");
                    return;
                }

                for (var y = 0; y < columnMap.Length; y++)
                {
                    var lineMap = columnMap[y];

                    for (int x = 0; x < columnWidth; x++)
                    {
                        var nonWhitespaceCharacterIsHere = (x < lineMap.Length) && lineMap[x];

                        resultImage[columnDrawX + x, y + headerHeight] = nonWhitespaceCharacterIsHere
                            ? (Rgb24)Color.Black
                            : HsvToRgb(currentLineHue,
                                DefaultBackgroundSaturation, 1d);
                    }

                    currentLineHue += hueStep;
                }

                columnDrawX += columnWidth;
            }
            
            new LongDirectoryInfo(LongPath.GetDirectoryName(mapSavePath)).Create();
            resultImage.SaveAsPng(mapSavePath);
        }

        private static string GetSavePath(string filePath) =>
            LongPath.Combine(LongPath.GetDirectoryName(filePath),
                LongPath.GetFileNameWithoutExtension(filePath) + "_ext",
                TextMapPath);

        // https://stackoverflow.com/a/1335465
        // but I've probably already copied this somewhere in Common or Imaging
        // ...or not. TODO: Add this to Celarix.Imaging, too
        private static Rgb24 HsvToRgb(double h,
            double S,
            double V)
        {
            var H = h;
            while (H < 0) { H += 360; }
            while (H >= 360) { H -= 360; }
            double R, G, B;
            if (V <= 0) { R = G = B = 0; }
            else if (S <= 0) { R = G = B = V; }
            else
            {
                var hf = H / 60.0;
                var i = (int)Math.Floor(hf);
                var f = hf - i;
                var pv = V * (1 - S);
                var qv = V * (1 - (S * f));
                var tv = V * (1 - (S * (1 - f)));
                switch (i)
                {
                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            
            return new Rgb24(Clamp((int)(R * 255d)),
                Clamp((int)(G * 255d)),
                Clamp((int)(B * 255d)));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        private static byte Clamp(int i) =>
            (byte)(i < 0
                ? 0
                : i > 255
                    ? 255
                    : i);
    }
}
