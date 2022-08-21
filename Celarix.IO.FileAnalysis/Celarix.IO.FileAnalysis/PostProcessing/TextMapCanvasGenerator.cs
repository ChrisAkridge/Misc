using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.Imaging.ZoomableCanvas;
using MoreLinq;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public static class TextMapCanvasGenerator
    {
        private const double DefaultBackgroundSaturation = 0.15d;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void GenerateTextMapCanvasForFile(string filePath, string outputFolderPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Drawing text map canvas for {filePath}...");
            
            using var stream = new StreamReader(LongFile.OpenRead(filePath));
            var lineMaps = new List<BitArray>();
            var totalImagePixels = 0L;
            var mappedLines = 0L;

            while (stream.ReadLine() is { } currentLine)
            {
                totalImagePixels += currentLine.Length;

                var lineMap = new BitArray(currentLine.Length);
                for (var i = 0; i < currentLine.Length; i++)
                {
                    lineMap[i] = !char.IsWhiteSpace(currentLine[i]);
                }
                lineMaps.Add(lineMap);

                mappedLines += 1;
                if (mappedLines % 10000L == 0L)
                {
                    logger.Info($"Generated line maps for {mappedLines:N0} lines. Total pixels so far: {totalImagePixels:N0}");
                }
            }

            var hueStep = 360d / lineMaps.Count;
            var linesPerColumn = (int)Math.Min(Math.Floor(Math.Sqrt(totalImagePixels)), lineMaps.Count);
            var batchedLineMaps = lineMaps
                .Batch(linesPerColumn)
                .Select(l => l.ToArray());
            var totalImageWidth = 0L;
            var startHue = 0d;
            var columnMaps = new List<TextMapColumn>();
            foreach (var batch in batchedLineMaps)
            {
                var columnWidth = batch.Max(l => l.Length);
                columnMaps.Add(new TextMapColumn
                {
                    Width = columnWidth,
                    X = totalImageWidth,
                    LineMaps = batch,
                    StartHue = startHue,
                    EndHue = startHue + (hueStep * linesPerColumn)
                });
                startHue += hueStep * linesPerColumn;

                totalImageWidth += columnWidth;
            }

            var level0CanvasTileWidth = (int)Math.Ceiling(totalImageWidth / 1024m);
            var level0CanvasTileHeight = (int)Math.Ceiling(linesPerColumn / 1024m);
            var level0TilesPath = LongPath.Combine(outputFolderPath, "0");
            LongDirectory.CreateDirectory(level0TilesPath);

            for (int y = 0; y < level0CanvasTileHeight; y++)
            {
                for (int x = 0; x < level0CanvasTileWidth; x++)
                {
                    logger.Info($"Drawing level 0 tile ({x}, {y})...");
                    var tileImage = DrawLevel0Tile(columnMaps, x, y);
                    var tileFolderPath = LongPath.Combine(outputFolderPath, "0", y.ToString());
                    LongDirectory.CreateDirectory(tileFolderPath);
                    tileImage.SaveAsPng(LongPath.Combine(tileFolderPath, $"{x}.png"));
                }
            }
            
            Utilities.Utilities.DrawZoomLevelsForLevel0CanvasTiles(level0TilesPath);
        }

        private static Image<Rgb24> DrawLevel0Tile(List<TextMapColumn> columnMaps, int tileX, int tileY)
        {
            var leftX = tileX * 1024L;
            var rightX = leftX + 1023L;
            var topY = tileY * 1024L;
            var bottomY = topY + 1023L;
            
            var leftColumnIndex = GetColumnIndexByXPosition(columnMaps, leftX);
            var rightColumnIndex = GetColumnIndexByXPosition(columnMaps, rightX);

            var tileImage = new Image<Rgb24>(1024, 1024, Color.Black);
            for (int i = leftColumnIndex; i <= rightColumnIndex; i++)
            {
                var column = columnMaps[i];
                var columnLeftRelativeToTile = column.X - leftX;
                var columnRightRelativeToTile = (column.X + column.Width) - leftX;
                var columnTopRelativeToTile = -topY;
                var columnBottomRelativeToTile = column.LineMaps.Length - topY;
                
                var endX = Math.Min(1023, columnRightRelativeToTile);
                var endY = Math.Min(1023, columnBottomRelativeToTile);
                for (var y = 0; y <= endY; y++)
                {
                    var x = (int)Math.Max(0, columnLeftRelativeToTile);
                    var lineMapIndex = y - columnTopRelativeToTile;
                    if (lineMapIndex >= column.LineMaps.Length)
                    {
                        break;
                    }
                    var lineMap = column.LineMaps[lineMapIndex];
                    for (; x <= endX; x++)
                    {
                        var characterIndex = x - columnLeftRelativeToTile;
                        var character = (characterIndex < lineMap.Length) && lineMap[(int)characterIndex];
                        var hue = column.GetHueForLine(y - (int)columnTopRelativeToTile);

                        tileImage[x, y] = character
                            ? (Rgb24)Color.Black
                            : HsvToRgb(hue, DefaultBackgroundSaturation, 1d);
                    }
                }
            }

            return tileImage;
        }

        private static int GetColumnIndexByXPosition(IReadOnlyList<TextMapColumn> columnMaps, long x)
        {
            var seenColumnsWidth = 0L;
            for (int i = 0; i < columnMaps.Count; i++)
            {
                var columnWidth = columnMaps[i].Width;
                seenColumnsWidth += columnWidth;
                if (seenColumnsWidth >= x)
                {
                    return i;
                }
            }

            return columnMaps.Count - 1;
        }

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
