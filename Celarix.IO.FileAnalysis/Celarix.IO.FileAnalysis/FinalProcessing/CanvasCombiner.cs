using Celarix.Imaging.ZoomableCanvas;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.IL;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
	public static class CanvasCombiner
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static void CombineCanvases(string[] canvasPaths, string outputPath)
		{
            LoggingConfigurer.ConfigurePostProcessingLogging();
			logger.Info($"Combining {canvasPaths.Length} canvases...");
			var blocks = canvasPaths
				.Select(p => new Block
				{
					Size = GetCanvasSizeInTiles(p),
					CanvasFolderPath = p
				})
                .OrderByDescending(b => b.Size.Width)
				.ToArray();

			var progress = new Progress<string>();
			progress.ProgressChanged += (_, e) => logger.Info(e);

			var packer = new Packer();
			packer.Fit(blocks, progress);

			foreach (var block in blocks)
			{
				MoveCanvasToOutput(block, outputPath);
			}
            
            throw new InvalidOperationException("Maybe zoom level generation should come later; if none of the lower-level 4 tiles exist on drive, skip generating this tile, too");
            // we don't need millions of blank tiles
            // Celarix.Imaging already has the fix for that, use that
            
			var currentZoomLevel = canvasPaths
				.Select(p => GetMaxZoomLevel(p))
				.Min() + 1;

			while (ZoomLevelGenerator.TryCombineImagesForNextZoomLevel(Path.Combine(outputPath, $"{currentZoomLevel - 1}"),
				Path.Combine(outputPath, $"{currentZoomLevel}"),
				currentZoomLevel,
				progress))
			{
				currentZoomLevel++;
			}
		}

		private static Size GetCanvasSizeInTiles(string canvasPath)
		{
			var maxZoomLevel = GetMaxZoomLevel(canvasPath);
			var tilesToAnEdge = (int)Math.Pow(2, maxZoomLevel);
            
            logger.Info($"Canvas {canvasPath} has a max zoom level of {maxZoomLevel}, overall size is {tilesToAnEdge} tiles squared");
            
			return new Size(tilesToAnEdge, tilesToAnEdge);
		}

		private static int GetMaxZoomLevel(string canvasPath)
        {
            if (canvasPath == "asm") { return 11; }
            
            var directories = Directory.GetDirectories(canvasPath, "*", SearchOption.TopDirectoryOnly);
            var zoomLevelNames = directories.Select(ParseZoomLevelName).ToList();

            return zoomLevelNames.Max();
        }

        private static int ParseZoomLevelName(string f)
        {
            return int.Parse(Path.GetFileName(f));
        }

        private static void MoveCanvasToOutput(Block block, string outputFolder)
		{
            logger.Info($"Moving canvas {block.CanvasFolderPath}...");
			var maxZoomLevel = GetMaxZoomLevel(block.CanvasFolderPath);

			for (var i = maxZoomLevel; i >= 0; i--)
            {
                var scaledLocation = new Point(block.Fit.Location.X >> i, block.Fit.Location.Y >> i);
                var scaledSize = new Size(block.Size.Width >> i, block.Size.Height >> i);
                // TODO: check if inputZoomLevelFolder being a zoom level and not the canvas path will work with GetRowFolderName
				var inputZoomLevelFolder = Path.Combine(block.CanvasFolderPath, i.ToString());
				var outputZoomLevelFolder = Path.Combine(outputFolder, i.ToString());

				var outputTopY = scaledLocation.Y;
				var outputBottomY = outputTopY + scaledSize.Height;
				for (var y = outputTopY; y < outputBottomY; y++)
				{
                    logger.Info($"Moving canvas at zoom level {i}, row {y} (limit {outputBottomY})");

                    var tileCopiedForRow = false;
                    var outputLeftX = scaledLocation.X;
					var outputRightX = outputLeftX + scaledSize.Width;

					var fullInputFolderPath = GetRowFolderName(Path.GetDirectoryName(inputZoomLevelFolder), i, (y - outputTopY));

                    if (!Directory.Exists(fullInputFolderPath))
                    {
                        logger.Info($"Whoops, there were no tiles on y={y}");
                        continue;
                    }
                    
					var fullOutputFolderPath = Path.Combine(outputZoomLevelFolder, y.ToString());
					Directory.CreateDirectory(fullOutputFolderPath);

					for (var x = outputLeftX; x < outputRightX; x++)
					{
						var inputTilePath = Path.Combine(fullInputFolderPath, $"{x - outputLeftX}.png");

                        if (!File.Exists(inputTilePath))
                        {
                            continue;
                        }
                        
						var outputTilePath = Path.Combine(fullOutputFolderPath, $"{x}.png");
                        File.Copy(inputTilePath, outputTilePath);
                        tileCopiedForRow = true;
                    }

                    if (!tileCopiedForRow)
                    {
                        logger.Info($"Whoops, there were no tiles on y={y}");
                        Directory.Delete(fullOutputFolderPath);
                    }
				}
			}
		}

        private static string GetRowFolderName(string canvasPath, int zoomLevel, int y)
        {
            return canvasPath != "asm"
                ? Path.Combine(canvasPath, zoomLevel.ToString(), y.ToString())
                : zoomLevel switch
                {
                    0 => Path.Combine(@"L:\textMapCanvases\asm\0", y.ToString()),
                    1 when y <= 362 => Path.Combine(@"K:\fa\textMapCanvases\asm\1", y.ToString()),
                    _ => Path.Combine(@"D:\fa\textMapCanvases\asm", zoomLevel.ToString(), y.ToString())
                };
        }
    }
}
