using Celarix.Imaging.ZoomableCanvas;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
	public static class CanvasCombiner
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static void CombineCanvases(string[] canvasPaths, string outputPath)
		{
			logger.Info($"Combining {canvasPaths.Length} canvases...");
			var blocks = canvasPaths
				.Select(p => new Block
				{
					Size = GetCanvasSizeInTiles(p),
					CanvasFolderPath = p
				})
				.ToArray();

			var progress = new Progress<string>();
			progress.ProgressChanged += (s, e) => logger.Info(e);

			var packer = new Packer();
			packer.Fit(blocks, progress);

			foreach (var block in blocks)
			{
				MoveCanvasToOutput(block, outputPath);
			}

			var currentZoomLevel = canvasPaths
				.Select(p => GetMaxZoomLevel(p))
				.Max() + 1;

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
			return new Size(tilesToAnEdge, tilesToAnEdge);
		}

		private static int GetMaxZoomLevel(string canvasPath)
		{
			return Directory.GetDirectories(canvasPath, "*", SearchOption.TopDirectoryOnly)
				.Select(f => int.Parse(Path.GetFileName(f)))
				.Max();
		}

		private static void MoveCanvasToOutput(Block block, string outputFolder)
		{
			var maxZoomLevel = GetMaxZoomLevel(block.CanvasFolderPath);

			for (var i = maxZoomLevel; i >= 0; i--)
			{
				var inputZoomLevelFolder = Path.Combine(block.CanvasFolderPath, i.ToString());
				var outputZoomLevelFolder = Path.Combine(outputFolder, i.ToString());

				var outputTopY = block.Fit.Location.Y;
				var outputBottomY = outputTopY + block.Fit.Size.Height;
				for (var y = outputTopY; y < outputBottomY; y++)
				{
					var outputLeftX = block.Fit.Location.X;
					var outputRightX = outputLeftX + block.Fit.Size.Width;

					var fullInputFolderPath = Path.Combine(inputZoomLevelFolder, (y - outputTopY).ToString());
					var fullOutputFolderPath = Path.Combine(outputZoomLevelFolder, y.ToString());
					Directory.CreateDirectory(fullOutputFolderPath);

					for (var x = outputLeftX; x < outputRightX; x++)
					{
						var inputTilePath = Path.Combine(fullInputFolderPath, $"{x - outputLeftX}.png");
						var outputTilePath = Path.Combine(fullOutputFolderPath, $"{x}.png");
						File.Move(inputTilePath, outputTilePath);
					}
				}
			}
		}
	}
}
