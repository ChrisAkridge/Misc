using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
	[Obsolete]
	public static class HeuristicSort
	{
		private const float ImageSizeWeight = 0.4f;
		private const float ImageComplexityWeight = 0.15f;
		private const float ImageIsColorWeight = 0.42f;

		private static int GetImageSize(Bitmap bitmap)
		{
			return bitmap.Width * bitmap.Height;
		}

		private static float GetImageBytesPerPixel(FileInfo info, Bitmap bitmap)
		{
			return (float)info.Length / GetImageSize(bitmap);
		}

		public static float GetImageScore(string filePath)
		{
			try
			{
				Bitmap bitmap = new Bitmap(Image.FromFile(filePath));

				float sizeScore = (GetImageSize(bitmap) / 1000000f) * ImageSizeWeight;
				float complexityScore = (GetImageBytesPerPixel(new FileInfo(filePath), bitmap)) * ImageComplexityWeight;

				return sizeScore + complexityScore;
			}
			catch (OutOfMemoryException ex)
			{
				return 0f;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public static IEnumerable<string> Sort(IEnumerable<string> filePaths)
		{
			return filePaths.OrderByDescending(f => GetImageScore(f));
		}
	}
}
