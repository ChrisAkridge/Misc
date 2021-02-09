using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PictureTilerCLI.Packer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PictureTilerCLI.MultiPicture
{
    public static class Divider
    {
        public static void Divide(IEnumerable<PositionedImage> images, Size cellSize, string outputFolderPath)
        {
            var level0Tiles = BuildTileMap(images, cellSize);
            Directory.CreateDirectory(Path.Combine(outputFolderPath, "0"));

            foreach (var level0Tile in level0Tiles)
            {
                SaveLevel0CellImage(level0Tile.Key, cellSize, level0Tile.Value, outputFolderPath);
            }

            int currentZoomLevel = 0;
            string folderToCombine;

            do
            {
                folderToCombine = Path.Combine(outputFolderPath, $"{currentZoomLevel}");
            } while (
                TryCombineImagesForNextZoomLevel(cellSize, folderToCombine, outputFolderPath, ++currentZoomLevel));
        }

        private static Dictionary<Point, List<PositionedImage>> BuildTileMap(IEnumerable<PositionedImage> images,
            Size cellSize)
        {
            var level0Tiles = new Dictionary<Point, List<PositionedImage>>();

            foreach (var image in images)
            {
                var corners = new List<Point>
                {
                    new Point(image.Position.X, image.Position.Y),
                    new Point((image.Position.X + image.Size.Width) - 1, image.Position.Y),
                    new Point(image.Position.X, (image.Position.Y + image.Size.Height) - 1),
                    new Point((image.Position.X + image.Size.Width) - 1, (image.Position.Y + image.Size.Height) - 1)
                };

                var cells = corners.Select(corner => new Point(corner.X / cellSize.Width, corner.Y / cellSize.Height)).ToList();
                var minCellX = cells.Min(c => c.X);
                var minCellY = cells.Min(c => c.Y);
                var maxCellX = cells.Max(c => c.X);
                var maxCellY = cells.Max(c => c.Y);

                for (int y = minCellY; y <= maxCellY; y++)
                {
                    for (int x = minCellX; x <= maxCellX; x++)
                    {
                        var cell = new Point(x, y);
                        if (!level0Tiles.ContainsKey(cell)) { level0Tiles.Add(cell, new List<PositionedImage>()); }

                        Point cellOrigin = new Point(cell.X * cellSize.Width, cell.Y * cellSize.Height);
                        var positionInCell =
                            new Point(image.Position.X - cellOrigin.X, image.Position.Y - cellOrigin.Y);

                        level0Tiles[cell]
                            .Add(new PositionedImage
                            {
                                ImageFilePath = image.ImageFilePath,
                                Position = positionInCell,
                                Size = image.Size
                            });
                    }
                }
            }

            return level0Tiles;
        }

        private static void SaveLevel0CellImage(Point cellNumber, Size cellSize, IList<PositionedImage> images, string outputFolderPath)
        {
            string outputFilePath = Path.Combine(outputFolderPath, "0", $"{cellNumber.X},{cellNumber.Y}.png");
            var cellImage = new Image<Rgba32>(cellSize.Width, cellSize.Height);

            foreach (var image in images)
            {
                using (var imageToDraw = Image.Load(image.ImageFilePath))
                {
                    cellImage.Mutate(ci => ci.DrawImage(imageToDraw, image.Position, 1f));
                }
            }

            cellImage.SaveAsPng(outputFilePath);
            cellImage.Dispose();
            Console.WriteLine($"Saved level 0 cell {cellNumber.X}, {cellNumber.Y}");
        }

        private static bool TryCombineImagesForNextZoomLevel(Size cellSize, string inputFolderPath, string outputFolderPath, int nextZoomLevel)
        {
            var paddingImage = new Image<Rgba32>(cellSize.Width, cellSize.Height, Rgba32.ParseHex("ffffffff"));

            var files = Directory.GetFiles(inputFolderPath, "*.png", SearchOption.TopDirectoryOnly)
                .Select(f =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(f);
                    var cellNumberParts = fileName.Split(',').Select(int.Parse).ToList();
                    return new
                    {
                        CellNumber = new Point(cellNumberParts[0], cellNumberParts[1]),
                        FilePath = f
                    };
                }).ToDictionary(a => a.CellNumber, a => a.FilePath);

            if (files.Count <= 1) { return false; }
            Directory.CreateDirectory(Path.Combine(outputFolderPath, $"{nextZoomLevel}"));

            int levelWidth = files.Max(kvp => kvp.Key.X);
            int levelHeight = files.Max(kvp => kvp.Key.Y);

            for (int y = 0; y < levelHeight; y += 2)
            {
                for (int x = 0; x < levelWidth; x += 2)
                {
                    var outputFilePath = Path.Combine(outputFolderPath, $"{nextZoomLevel}", $"{x / 2},{y / 2}.png");

                    var topLeftCell = new Point(x, y);
                    var topRightCell = new Point(x + 1, y);
                    var bottomLeftCell = new Point(x, y + 1);
                    var bottomRightCell = new Point(x + 1, y + 1);

                    var topLeft = (files.ContainsKey(topLeftCell)) ? Image.Load(files[topLeftCell]) : paddingImage;
                    var topRight = (files.ContainsKey(topRightCell)) ? Image.Load(files[topRightCell]) : paddingImage;
                    var bottomLeft = (files.ContainsKey(bottomLeftCell)) ? Image.Load(files[bottomLeftCell]) : paddingImage;
                    var bottomRight = (files.ContainsKey(bottomRightCell)) ? Image.Load(files[bottomRightCell]) : paddingImage;

                    var canvas = new Image<Rgba32>(cellSize.Width * 2, cellSize.Height * 2, Rgba32.ParseHex("ffffffff"));
                    canvas.Mutate(c => c.DrawImage(topLeft, Point.Empty, 1f));
                    canvas.Mutate(c => c.DrawImage(topRight, new Point(cellSize.Width, 0), 1f));
                    canvas.Mutate(c => c.DrawImage(bottomLeft, new Point(0, cellSize.Height), 1f));
                    canvas.Mutate(c => c.DrawImage(bottomRight, new Point(cellSize.Width, cellSize.Height), 1f));
                    canvas.Mutate(c => c.Resize(cellSize));
                    canvas.SaveAsPng(outputFilePath);

                    if (topLeft != paddingImage) { topLeft.Dispose(); }
                    if (topRight != paddingImage) { topRight.Dispose(); }
                    if (bottomLeft != paddingImage) { bottomLeft.Dispose(); }
                    if (bottomRight != paddingImage) { bottomRight.Dispose(); }
                    canvas.Dispose();

                    Console.WriteLine($"Saved level {nextZoomLevel} cell {x / 2}, {y / 2}");
                }
            }

            return true;
        }
    }
}
