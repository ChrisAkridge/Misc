using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessorCore;

namespace PictureTilerCLI
{
	internal static class Tiler
	{
		public static void Test(Options options)
		{
			var files = GetFiles(options.InputFolderPath, options.OrderBy);

			var canvas = CreateCanvas(files.Count(), new Size(options.Width, options.Height));
		}

		public static void Tile(Options options)
		{
			var files = GetFiles(options.InputFolderPath, options.OrderBy);
			var imagesOnCanvas = GetPictureCountOnCanvas(files.Count());
			var canvas = CreateCanvas(files.Count(), new Size(options.Width, options.Height));
			var aspect = GetAspectRatio(options.Width, options.Height);

			int x = 0;
			int y = 0;
			int widthInImages = imagesOnCanvas.Item1;
			int processed = 0;

			foreach (var file in files)
			{
				using (FileStream inputStream = File.OpenRead(file))
				{
					try
					{
						if (processed == 1980) { System.Diagnostics.Debugger.Break();  }
						var inputImage = new Image(inputStream);
						var cropRect = GetImageCropRect(inputImage.Width, inputImage.Height, aspect.Item1, aspect.Item2);
						var cropped = CropImage(inputImage, cropRect);
						var resized = ResizeImage(cropped, new Size(options.Width, options.Height));
						Point destination = new Point(x * options.Width, y * options.Height);
						Console.WriteLine($"Drawing image {processed} at {destination}");
						CustomBlend.Blend(canvas, resized, destination);
					}
					catch (Exception ex) { Console.WriteLine(ex.Message); continue; }
					processed++;
				}

				if (x < widthInImages - 1) { x++; }
				else
				{
					x = 0;
					y++;
				}
			}

			using (FileStream outStream = File.OpenWrite(options.OutputFilePath))
			{
				switch (options.OutputFileType)
				{
					case OutputFileTypes.Default:
						throw new ArgumentException();
					case OutputFileTypes.Gif:
						canvas.SaveAsGif(outStream);
						break;
					case OutputFileTypes.Jpeg:
						canvas.SaveAsJpeg(outStream, 99);
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
	
		private static IEnumerable<string> GetFiles(string inputFolderPath, OrderByTypes orderByType)
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

			switch (orderByType)
			{
				case OrderByTypes.Default:
				case OrderByTypes.Name:
					return filteredFiles;
				case OrderByTypes.DateCreated:
					return filteredFiles.OrderBy(f =>
					{
						FileInfo fi = new FileInfo(f);
						return fi.CreationTime;
					});
				case OrderByTypes.Size:
					return filteredFiles.OrderBy(f =>
					{
						FileInfo fi = new FileInfo(f);
						return fi.Length;
					});
				default:
					return filteredFiles;
			}
		}

		private static Tuple<int, int> GetPictureCountOnCanvas(int pictureCount)
		{
			double sqrt = Math.Sqrt(pictureCount);
			double decimalPart = sqrt - Math.Truncate(sqrt);
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

				Point imageCenter = GetImageCenter(new Size(imageWidth, imageHeight));

				Size cropSizeBasedOnHeight = new Size((int)(imageHeight * oneByAspect), imageHeight);
				Size cropSizeBasedOnWidth = new Size(imageWidth, (int)(imageWidth / oneByAspect));
				Size cropSize = (WillSizeFit(cropSizeBasedOnHeight, new Size(imageWidth, imageHeight))) ? cropSizeBasedOnHeight : cropSizeBasedOnWidth;
				Point cropTopLeft = new Point(imageCenter.X - (cropSize.Width / 2), imageCenter.Y - (cropSize.Height / 2));
				return new Rectangle(cropTopLeft.X, cropTopLeft.Y, cropSize.Width, cropSize.Height);
			}
		}

		private static Image<Color, uint> CropImage(Image original, Rectangle cropRect)
		{
			return original.Crop(cropRect.Width, cropRect.Height, cropRect);
		}

		private static Image<Color, uint> ResizeImage(Image<Color, uint> image, Size newSize)
		{
			return image.Resize(newSize.Width, newSize.Height);
		}

		private static Point GetImageCenter(Size imageSize)
		{
			return new Point(imageSize.Width / 2, imageSize.Height / 2);
		}

		private static Size GetCanvasSize(int imageCount, Size imageSize)
		{
			var countOnCanvas = GetPictureCountOnCanvas(imageCount);
			return new Size(countOnCanvas.Item1 * imageSize.Width, countOnCanvas.Item2 * imageSize.Height);
		}

		private static Image CreateCanvas(int imageCount, Size imageSize)
		{
			var canvasSize = GetCanvasSize(imageCount, imageSize);
			Image result = new Image(canvasSize.Width, canvasSize.Height);
			using (PixelAccessor<Color, uint> pixels = result.Lock())
			{
				for (int y = 0; y < imageSize.Height; y++)
				{
					for (int x = 0; x < imageSize.Width; x++)
					{
						pixels[x, y] = Color.White;
					}
				}
			}

			return result;
		}

		private static void OverlayImage(Image canvas, Image<Color, uint> image, int x, int y, Size imageSize)
		{
			Point imageOverlayPosition = new Point(x * imageSize.Width, y * imageSize.Height);
			canvas.Blend(image, 100, new Rectangle(imageOverlayPosition, imageSize));
		}

		private static bool WillSizeFit(Size a, Size b)
		{
			return a.Width <= b.Width && a.Height <= b.Height;
		}

		private static Tuple<int, int> GetAspectRatio(int width, int height)
		{
			int gcd = GetGCD(height, width);
			int a = height / gcd;
			int b = width / gcd;
			return new Tuple<int, int>(b, a);
		}

		private static double GetOneByAspectRatio(int width, int height)
		{
			return (double)width / height;
		}

		private static int GetGCD(int a, int b)
		{
			// http://stackoverflow.com/a/2565188/2709212
			return b == 0 ? a : GetGCD(b, a % b);
		}
	}
}
