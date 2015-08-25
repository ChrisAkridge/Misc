using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTools
{
	internal class Compressor
	{
		// Compression algorithm is based on the Toy Compression Algorithm found at http://www.zutopedia.com/compression.html
		private string text;
		private Dictionary<string, string> compressionDictionary;
		private int compressionKeyLength = 1;

		public Compressor(string text)
		{
			text = text;
		}

		public Compressor(byte[] data) : this(BaseWriter.BytesToOffsetHexString(data)) { }

		public string FindLongestPattern()
		{
			int minPatternLength = compressionKeyLength + 1;
			int maxPatternLength = text.Length / 2;
			List<Tuple<string, int>> overAllPatterns = new List<Tuple<string, int>>();
			List<string> localPatterns;

			for (int length = minPatternLength; length < maxPatternLength; length++)
			{
				localPatterns = new List<string>();
				for (int i = 0; i < (text.Length / 2) - (maxPatternLength - 1); i++)
				{
					localPatterns.Add(text.Substring(i, length));
				}

				overAllPatterns.AddRange(localPatterns.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => new Tuple<string, int>(g.First(), g.Count())));
			}

			return overAllPatterns.OrderBy(p => p.Item2).FirstOrDefault()?.Item1;
		}
	}

	internal class Pattern
	{
		public byte[] Value { get; private set; }
		public int Occurrences { get; private set; }

		public int Length
		{
			get
			{
				return Value.Length;
			}
		}

		public Pattern(byte[] value, int occurrences)
		{
			Value = value;
			Occurrences = occurrences;
		}
	}
}
