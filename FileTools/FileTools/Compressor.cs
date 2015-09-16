using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileTools
{
	public sealed class StringCompressor
	{
		// Compression algorithm is based on the Toy Compression Algorithm found at http://www.zutopedia.com/compression.html
		private const int DictionaryValueLengthSize = 1;

		private byte[] utf8Cache;
		private int currentKey = -1;    // a dictonary entry is written to disk as {key}, {length of value}, {value}. The {length of value} is always one byte.

		public int KeyLength { get; private set; }
		private bool keyLengthExpansionPerformed = false;
		private int maxValueLength;
		private Dictionary<int, string> compressionDictionary;
		private bool fullyCompressed = false;
		public event DictionaryEntryAdded DictionaryEntryAddedEvent;

		private int compressionStep;

		private string initialData;
		public string CurrentKey
		{
			get
			{
				return GetKeyText(currentKey);
			}
		}
		public string Data { get; private set; }

		public IReadOnlyList<byte> UTF8Bytes
		{
			get
			{
				return utf8Cache.ToList().AsReadOnly();
			}
		}

		public IReadOnlyDictionary<int, string> CompressionDictionary
		{
			get
			{
				return new ReadOnlyDictionary<int, string>(compressionDictionary);
			}
		}
		public event KeyLengthExpansionOccurredEventHandler KeyLengthExpansionOccurred;

		private StringCompressor(string cInitialData, int cMaxValueLength, int cKeyLength)
		{
			if (string.IsNullOrEmpty(cInitialData)) throw new ArgumentNullException(nameof(cInitialData), "A null value was passed to the compressor.");
			if (cMaxValueLength <= 0 || cMaxValueLength > 255) throw new ArgumentOutOfRangeException(nameof(cMaxValueLength), $"The maximum value length must be between 1 and 255 inclusive. It was {cMaxValueLength}.");
			if (cKeyLength <= 0 || cKeyLength > 4) throw new ArgumentOutOfRangeException(nameof(cKeyLength), $"The key length must be between 1 and 4 inclusive. Got {cKeyLength}.");

			Data = cInitialData;
			initialData = cInitialData;
			UpdateUTF8Cache();
			compressionDictionary = new Dictionary<int, string>();
			maxValueLength = cMaxValueLength;
			KeyLength = cKeyLength;
		}

		public StringCompressor(string initialData, int cMaxValueLength = 64) : this(initialData, cMaxValueLength, 1) { }

		public void Compress()
		{
			while (!fullyCompressed)
			{
				PerformCompressionStep();
			}
		}

		public void BatchCompress()
		{
			while (!fullyCompressed)
			{
				PerformBatchCompressionStep();
			}
		}

		public bool PerformCompressionStep()
		{
			if (fullyCompressed) return true;

			string bestPattern = FindBestPattern();

			if (bestPattern != null)
			{
				PerformSubstitution(bestPattern);
			}
			else
			{
				fullyCompressed = true;
			}

			compressionStep++;
			Console.WriteLine($"Compression step {compressionStep} has completed. Current size: {utf8Cache.Length} bytes ({((decimal)utf8Cache.Length / (decimal)initialData.Length * 100m)}%). Dictionary has {compressionDictionary.Count} entries.");
			return fullyCompressed;
		}

		public void PerformBatchCompressionStep()
		{
			if (fullyCompressed) return;

			var bestPatterns = FindBestPatterns();

			if (bestPatterns.Any())
			{
				for (int i = 0; i < bestPatterns.Count; i++)
				{
					if (i % 1000 == 0) Console.WriteLine($"{DateTime.Now} Wrote {i} patterns");
					PerformSubstitution(bestPatterns[i]);
				}
			}
			else
			{
				fullyCompressed = true;
			}

			compressionStep++;
			Console.WriteLine($"Compression step {compressionStep} has completed. Current size: {utf8Cache.Length} bytes ({((decimal)utf8Cache.Length / (decimal)initialData.Length * 100m)}%). Dictionary has {compressionDictionary.Count} entries.");
		}

		public void WriteToDisk(string path)
		{
			WriteToDisk(path, Data, compressionDictionary, utf8Cache, KeyLength);
		}

		internal static void WriteToDisk(string path, string data, Dictionary<int, string> argCompressionDictionary, byte[] argUTF8Cache, int argKeyLength)
		{
			using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
			{
				writer.Write((uint)argUTF8Cache.Length);
				writer.Write(argUTF8Cache);
				writer.Write((byte)argKeyLength);

				writer.Write(new char[] { 'D', 'I', 'C', 'T' }, 0, 4);
				foreach (var entry in argCompressionDictionary)
				{
					if (argKeyLength == 1) { writer.Write((byte)entry.Key); }
					else if (argKeyLength == 2) { writer.Write((ushort)entry.Key); }
					else if (argKeyLength == 3)
					{
						writer.Write((byte)((entry.Key & 0xFF0000) >> 16));
						writer.Write((byte)((entry.Key & 0xFF00) >> 8));
						writer.Write((byte)(entry.Key & 0xFF));
					}
					else if (argKeyLength == 4) { writer.Write((uint)entry.Key); }

					writer.Write((byte)entry.Value.Length);
					writer.Write(Encoding.UTF8.GetBytes(entry.Value));
				}
			}
		}

		private string FindBestPattern()
		{
			var substringOccurrences = new Dictionary<string, int>();
			int minValueLength = KeyLength + 1;

			for (int length = minValueLength; length <= maxValueLength; length++)
			{
				for (int i = 0; i < (Data.Length - 1 - length); i++)
				{
					string substring = Data.Substring(i, length);

					if (!substringOccurrences.ContainsKey(substring)) { substringOccurrences.Add(substring, 1); }
					else { substringOccurrences[substring]++; }
				}
			}

			var selectedSubstrings = substringOccurrences.Where(s => s.Value > 1);
			var substringsBySavings = selectedSubstrings.Select(s => new Tuple<string, int>(s.Key, GetSubstitutionSavings(s.Key.Length, s.Value))).Where(s => s.Item2 > 0);

			var value = substringsBySavings.OrderByDescending(s => s.Item2).FirstOrDefault()?.Item1;

			return value;
		}

		private List<string> FindBestPatterns()
		{
			var substringOccurrences = new Dictionary<string, int>();
			int minValueLength = KeyLength + 1;

			for (int length = minValueLength; length <= maxValueLength; length++)
			{
				for (int i = 0; i < (Data.Length - 1 - length); i++)
				{
					string substring = Data.Substring(i, length);

					if (!substringOccurrences.ContainsKey(substring)) { substringOccurrences.Add(substring, 1); }
					else { substringOccurrences[substring]++; }
				}
			}

			var selectedSubstrings = substringOccurrences.Where(s => s.Value > 1);
			var substringsBySavings = selectedSubstrings.Select(s => new Tuple<string, int>(s.Key, GetSubstitutionSavings(s.Key.Length, s.Value))).Where(s => s.Item2 > 0);
			var result = substringsBySavings.OrderByDescending(s => s.Item2).Select(s => s.Item1);

			return result.ToList();
		}

		private static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
		{
			for (int i = 0; i < str.Length; i += maxChunkSize)
			{
				yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
			}
		}

		private void PerformSubstitution(string pattern)
		{
			currentKey = GetNextKey();
			if (keyLengthExpansionPerformed)
			{
				keyLengthExpansionPerformed = false;
				return;
			}

			compressionDictionary.Add(currentKey, pattern);
			if (DictionaryEntryAddedEvent != null) DictionaryEntryAddedEvent(this, new KeyValuePair<int, string>(currentKey, pattern));
			Data = Data.Replace(pattern, CurrentKey);
			UpdateUTF8Cache();
		}

		private int GetSubstitutionSavings(string pattern)
		{
			int patternLength = pattern.Length;
			int occurrences = Regex.Matches(Data, Regex.Escape(pattern)).Count;

			return GetSubstitutionSavings(patternLength, occurrences);
		}

		private int GetSubstitutionSavings(int patternLength, int occurrences)
		{
			int substitutionSavings = (patternLength * occurrences) + (occurrences * KeyLength);
			int dictionaryCost = KeyLength + DictionaryValueLengthSize + patternLength;

			return substitutionSavings - dictionaryCost;
		}

		private void UpdateUTF8Cache()
		{
			utf8Cache = Encoding.UTF8.GetBytes(Data);
		}

		internal static string GetKeyText(int key, int argKeyLength)
		{
			char a = (char)((key & 0xFF000000) >> 24);
			char b = (char)((key & 0x00FF0000) >> 16);
			char c = (char)((key & 0x0000FF00) >> 8);
			char d = (char)(key & 0x000000FF);

			string entireKey = new string(new char[] { a, b, c, d });
			return entireKey.Substring((4 - argKeyLength), argKeyLength);
		}

		private string GetKeyText(int key)
		{
			return GetKeyText(key, KeyLength);
		}

		private int GetNextKey()
		{
			int possibleNextKey = currentKey + 1;

			while (ContainsKey(possibleNextKey) && KeyWithinRange(possibleNextKey))
			{
				possibleNextKey++;
			}

			if (!KeyWithinRange(possibleNextKey))
			{
				ExpandKeySize();
				return 0;
			}

			return possibleNextKey;
		}

		private bool ContainsKey(int key)
		{
			if (KeyLength == 1)
			{
				return FastArrayTools.FastContains8(utf8Cache, unchecked((byte)key));
			}
			else if (KeyLength == 2)
			{
				return FastArrayTools.FastContains16(utf8Cache, unchecked((ushort)key));
			}
			else if (KeyLength == 3)
			{
				return FastArrayTools.FastContains24(utf8Cache, unchecked((uint)key));
			}
			else if (KeyLength == 4)
			{
				return FastArrayTools.FastContains32(utf8Cache, unchecked((uint)key));
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private bool KeyWithinRange(int key)
		{
			if (KeyLength == 1 && key <= 255) return true;
			else if (KeyLength == 2 && key <= 65535) return true;
			else if (KeyLength == 3 && key <= 1677215) return true;
			else if (KeyLength == 4 && key <= int.MaxValue) return true;

			return false;
		}

		private void ExpandKeySize()
		{
			if (KeyLength == 4) { throw new OverflowException($"Your file required more than ${compressionDictionary.Count} keys. We've ran out of key space!");  }

			Data = initialData;
			UpdateUTF8Cache();
			compressionDictionary.Clear();
			currentKey = 0;
			KeyLength++;
			keyLengthExpansionPerformed = true;
			OnKeyLengthExpansionOccurred();
		}

		private void OnKeyLengthExpansionOccurred()
		{
			if (KeyLengthExpansionOccurred != null)
			{
				KeyLengthExpansionOccurred(this, EventArgs.Empty);
			}
		}
	}

	public delegate void KeyLengthExpansionOccurredEventHandler(object sender, EventArgs e);
}
