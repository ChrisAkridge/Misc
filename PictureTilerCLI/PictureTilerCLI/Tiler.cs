using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PictureTilerCLI
{
    internal static class Tiler
    {
        public static void Test(TileOptions tileOptions)
        {
            var files = GetFiles(tileOptions.InputFolderPath, tileOptions.OrderBy);

            var canvas = CreateCanvas(files.Count(), new Size(tileOptions.Width, tileOptions.Height));
        }

        public static void Tile(TileOptions tileOptions)
        {
            var files = GetFiles(tileOptions.InputFolderPath, tileOptions.OrderBy);
            var imagesOnCanvas = GetPictureCountOnCanvas(files.Count());
            var canvas = CreateCanvas(files.Count(), new Size(tileOptions.Width, tileOptions.Height));
            var aspect = GetAspectRatio(tileOptions.Width, tileOptions.Height);

            int x = 0;
            int y = 0;
            int widthInImages = imagesOnCanvas.Item1;
            int processed = 0;

            foreach (var file in files)
            {
                using (var inputStream = File.OpenRead(file))
                {
                    try
                    {
                        if (processed == 1980) { System.Diagnostics.Debugger.Break(); }

                        var inputImage = Image.Load(inputStream);

                        var cropRect = GetImageCropRect(inputImage.Width, inputImage.Height, aspect.Item1,
                            aspect.Item2);
                        var cropped = CropImage(inputImage, cropRect);
                        var resized = ResizeImage(cropped, new Size(tileOptions.Width, tileOptions.Height));
                        var destination = new Point(x * tileOptions.Width, y * tileOptions.Height);
                        Console.WriteLine($"Drawing image {processed} at {destination}");
                        OverlayImage(canvas, resized, destination.X, destination.Y, new Size(tileOptions.Width, tileOptions.Height));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message); 
                        continue;
                    }
                    processed++;
                }

                if (x < widthInImages - 1) { x++; }
                else
                {
                    x = 0;
                    y++;
                }
            }

            using (var outStream = File.OpenWrite(tileOptions.OutputFilePath))
            {
                switch (tileOptions.OutputFileType)
                {
                    case OutputFileTypes.Default:
                        throw new ArgumentException();
                    case OutputFileTypes.Gif:
                        canvas.SaveAsGif(outStream);
                        break;
                    case OutputFileTypes.Jpeg:
                        canvas.SaveAsJpeg(outStream);
                        break;
                    case OutputFileTypes.Png:
                        canvas.SaveAsPng(outStream);
                        break;
                    default:
                        break;
                }
            }

            Console.ReadKey(intercept: true);
        }
    
        private static IList<string> GetFiles(string inputFolderPath, OrderByTypes orderByType)
        {
            if (!Directory.Exists(inputFolderPath))
            {
                throw new DirectoryNotFoundException($"Invalid path {inputFolderPath}: path does not exist");
            }

            var files = Directory.GetFiles(inputFolderPath, "*", SearchOption.TopDirectoryOnly);
            var filteredFiles = files.Where(f => f.EndsWith("gif", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith("jpeg", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith("png", StringComparison.InvariantCultureIgnoreCase));

            IEnumerable<string> resultFiles;

            switch (orderByType)
            {
                case OrderByTypes.DateCreated:
                    resultFiles = filteredFiles.OrderBy(f =>
                    {
                        var fi = new FileInfo(f);
                        return fi.CreationTime;
                    });
                    break;
                case OrderByTypes.Size:
                    resultFiles = filteredFiles.OrderBy(f =>
                    {
                        var fi = new FileInfo(f);
                        return fi.Length;
                    });
                    break;
                case OrderByTypes.Default:
                case OrderByTypes.Name:
                default:
                    resultFiles = filteredFiles;
                    break;
            }

            return resultFiles.ToList();
        }

        private static Tuple<int, int> GetPictureCountOnCanvas(int pictureCount)
        {
            double sqrt = Math.Sqrt(pictureCount);
            int intSqrt = (int)Math.Sqrt(pictureCount);

            if (intSqrt * intSqrt == pictureCount) { return new Tuple<int, int>(intSqrt, intSqrt); }
            else
            {
                // Code from ByteView
                // Used with permission
                int height = intSqrt;
                int remainder = pictureCount - (intSqrt * intSqrt);
                for (int remainderRows = (int)Math.Ceiling((double)remainder / intSqrt); remainderRows > 0; remainderRows--)
                {
                    height++;
                }
                return new Tuple<int, int>(intSqrt, height);
            }
        }

        private static Rectangle GetImageCropRect(int imageWidth, int imageHeight, int aspectX, int aspectY)
        {
            var imageAspect = GetAspectRatio(imageWidth, imageHeight);
            int imageAspectX = imageAspect.Item1;
            int imageAspectY = imageAspect.Item2;

            if (imageAspectX == aspectX && imageAspectY == aspectY)
            {
                return new Rectangle(0, 0, imageWidth, imageHeight);
            }
            else
            {
                double imageOneByAspect = (double)imageAspectX / imageAspectY;
                double oneByAspect = (double)aspectX / aspectY;

                var imageCenter = GetImageCenter(new Size(imageWidth, imageHeight));

                var cropSizeBasedOnHeight = new Size((int)(imageHeight * oneByAspect), imageHeight);
                var cropSizeBasedOnWidth = new Size(imageWidth, (int)(imageWidth / oneByAspect));
                var (width, height) = WillSizeFit(cropSizeBasedOnHeight, new Size(imageWidth, imageHeight)) 
                    ? cropSizeBasedOnHeight
                    : cropSizeBasedOnWidth;
                var (x, y) = new Point(imageCenter.X - (width / 2), imageCenter.Y - (height / 2));
                return new Rectangle(x, y, width, height);
            }
        }

        private static Image CropImage(Image original, Rectangle cropRect) => original.Clone(i => i.Crop(cropRect));

        private static Image ResizeImage(Image image, Size newSize) => image.Clone(i => i.Resize(newSize));

        private static Point GetImageCenter(Size imageSize) => new Point(imageSize.Width / 2, imageSize.Height / 2);

        private static Size GetCanvasSize(int imageCount, Size imageSize)
        {
            var countOnCanvas = GetPictureCountOnCanvas(imageCount);
            return new Size(countOnCanvas.Item1 * imageSize.Width, countOnCanvas.Item2 * imageSize.Height);
        }

        private static Image CreateCanvas(int imageCount, Size imageSize)
        {
            var canvasSize = GetCanvasSize(imageCount, imageSize);
            return new Image<Rgba32>(imageSize.Width, imageSize.Height, Rgba32.ParseHex("ffffffff"));
        }

        private static void OverlayImage(Image canvas, Image image, int x, int y, Size imageSize)
        {
            var imageOverlayPosition = new Point(x * imageSize.Width, y * imageSize.Height);
            canvas.Mutate(c => c.DrawImage(image, imageOverlayPosition, 1f));
        }

        private static bool WillSizeFit(Size a, Size b) => a.Width <= b.Width && a.Height <= b.Height;

        private static Tuple<int, int> GetAspectRatio(int width, int height)
        {
            int gcd = GetGCD(height, width);
            int a = height / gcd;
            int b = width / gcd;
            return new Tuple<int, int>(b, a);
        }

        private static double GetOneByAspectRatio(int width, int height) => (double)width / height;

        // http://stackoverflow.com/a/2565188/2709212
        private static int GetGCD(int a, int b)
        {
            while (true)
            {
                if (b == 0)
                {
                    return a;
                }

                var c = a;
                a = b;
                b = c % b;
            }
        }
    }
}
