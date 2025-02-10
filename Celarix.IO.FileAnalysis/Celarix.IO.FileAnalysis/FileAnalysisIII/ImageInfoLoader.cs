using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SixLabors.ImageSharp;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class ImageInfoLoader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public bool TryGetInfo(string path, out ImageInfo info)
		{
			try
			{
				var identifiedImage = Image.Identify(path);
				info = new ImageInfo
				{
					Width = identifiedImage.Width,
					Height = identifiedImage.Height
				};
				logger.Info($"{path} (image): {info.Width}x{info.Height}");
				return true;
			}
			catch (Exception ex)

			{
				info = null;
				return false;
			}
		}
	}
}
