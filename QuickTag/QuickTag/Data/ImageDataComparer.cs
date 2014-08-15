using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTag.Data
{
	public sealed class ImageDataComparer : IComparer<ImageData>
	{
		public int Compare(ImageData x, ImageData y)
		{
			string xFileName = Path.GetFileName(x.ImagePath);
			string yFileName = Path.GetFileName(y.ImagePath);
			return xFileName.CompareTo(yFileName);
		}
	}
}
