using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class AudioInfo
	{
		public int SampleRate { get; set; }
		public int Channels { get; set; }
		public TimeSpan Duration { get; set; }
	}
}
