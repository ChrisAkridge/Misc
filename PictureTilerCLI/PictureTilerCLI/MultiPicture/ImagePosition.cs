using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace PictureTilerCLI.MultiPicture
{
	public sealed class PositionedImage
	{
		public string ImageFilePath { get; set; }
		public Point Position { get; set; }
		public Size Size { get; set; }
	}
}
