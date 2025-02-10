using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class AnimatedImageOrVideoInfo
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public int FrameCount { get; set; }
		public TimeSpan Duration { get; set; }
	}
}
