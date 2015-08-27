using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTools
{
	public sealed class FastCompressor
	{
		// A faster version of StringCompressor that trades way less redundancy for less step-through ability.
		private const int DictionaryValueLengthSize = 1;

		private byte[] utf8Cache;
		private int currentKey = -1;
		private int keyLength;
		private bool keyLengthExpansionPerformed = false;
		private int maxValueLength;
		private Dictionary<int, string> compressionDictionary = new Dictionary<int, string>();
		private bool fullyCompressed = false;
		private int compressionStep;

		private string initialData;
		public string CurrentKey
		{
			get
			{
				return StringCompressor.GetKeyText(currentKey, keyLength);
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

		private FastCompressor(string cInitialData, int cMaxValueLength, int cKeyLength)
		{
			Data = cInitialData;
			initialData = cInitialData;
			UpdateUTF8Cache();
			maxValueLength = cMaxValueLength;
			keyLength = cKeyLength;
		}

		public FastCompressor(string initialData, int cMaxValueLength = 64) : this(initialData, cMaxValueLength, 1) { }

		public void Compress()
		{
			while (!fullyCompressed)
			{
				PerformCompressionStep();
			}
		}
		
		public void PerformCompressionStep()
		{
			if (fullyCompressed) return;
		
			var invalidKeys = GetInvalidKeyValues();
			var compressablePatterns = FindCompressablePatterns();
			var localCompressionDictionary = new Dictionary<int, string>(); // memory is cheap, right?
			var compressionKeysUsed = new List<int>();

			if (!compressablePatterns.Any())
			{
				fullyCompressed = true;
				return;
			}

			// Assign valid keys to each pattern
			foreach (string pattern in compressablePatterns)
			{
				int key = GetNextKey(invalidKeys);
				if (keyLengthExpansionPerformed)
				{
					keyLengthExpansionPerformed = false;	
					goto printCompressionStep;
				}
				localCompressionDictionary.Add(key, pattern);
				currentKey = key;
			}

			// Perform a bunch of string replacements
			foreach (var pattern in localCompressionDictionary)
			{
				string keyText = StringCompressor.GetKeyText(pattern.Key, keyLength);

				if (Data.Contains(pattern.Value))
				{
					Data = Data.Replace(pattern.Value, keyText);
					compressionKeysUsed.Add(pattern.Key);
				}
			}

			// Update our UTF-8 cache. We only needed to do this once.
			UpdateUTF8Cache();

			// Add keys used to our compression dictionary.
			compressionKeysUsed.ForEach(k => compressionDictionary.Add(k, localCompressionDictionary[k]));

		printCompressionStep:
			compressionStep++;
			Console.WriteLine($"Compression step {compressionStep} has completed. Current size: {utf8Cache.Length} bytes ({((decimal)utf8Cache.Length / (decimal)initialData.Length * 100m)}%). Dictionary has {compressionDictionary.Count} entries.");
		}

		public void WriteToDisk(string path)
		{
			StringCompressor.WriteToDisk(path, Data, compressionDictionary, utf8Cache, keyLength);
		}

		private List<string> FindCompressablePatterns()
		{
			var substringOccurrences = new Dictionary<string, int>();   // substring, number of occurrences
			int minValueLength = keyLength + 1;

			for (int length = minValueLength; length <= maxValueLength; length++)
			{
				for (int i = 0; i < (Data.Length - 1 - length); i += length)
				{
					string substring = Data.Substring(i, Math.Min(length, Data.Length - i));
					
					if (!substringOccurrences.ContainsKey(substring)) { substringOccurrences.Add(substring, 1); }
					else { substringOccurrences[substring]++; }
				}
			}

			var repeatedSubstrings = substringOccurrences.Where(s => s.Value > 1);
			var substringsBySavings = repeatedSubstrings.Select(s => new Tuple<string, int>(s.Key, GetSubstitutionSavings(s.Key.Length, s.Value))).Where(s => s.Item2 > 0);
			var result = substringsBySavings.OrderByDescending(s => s.Item2).Select(s => s.Item1);

			return result.ToList();
		}

		private List<int> GetInvalidKeyValues()
		{
			if (keyLength == 1)
			{
				return GetInvalidKeyValues8();
			}
			else if (keyLength == 2)
			{
				return FastArrayTools.FastUnique16(utf8Cache);
			}
			else if (keyLength == 3)
			{
				return FastArrayTools.FastUnique24(utf8Cache);
			}
			else if (keyLength == 4)
			{
				return FastArrayTools.FastUnique32(utf8Cache).ToList();
			}
			throw new InvalidOperationException();
		}

		private List<int> GetInvalidKeyValues8()
		{
			bool[] hasFoundByte = new bool[256];

			foreach (byte b in utf8Cache)
			{
				if (!hasFoundByte[b]) hasFoundByte[b] = true;
			}

			List<int> result = new List<int>(256);
			for (int i = 0; i < 256; i++)
			{
				if (hasFoundByte[i]) result.Add(i);
			}

			return result;
		}

		private int GetNextKey(List<int> invalidKeys)
		{
			int possibleNextKey = currentKey + 1;

			while (invalidKeys.Contains(possibleNextKey) && KeyWithinRange(possibleNextKey))
			{
				possibleNextKey++;	// TODO: could probably optimize by for-looping and grabbing a valid key next to an invalid one
			}

			if (!KeyWithinRange(possibleNextKey))
			{
				ExpandKeySize();
				return 0;
			}

			return possibleNextKey;
		}

		private bool KeyWithinRange(int key)
		{
			if (keyLength == 1 && key <= 255) return true;
			else if (keyLength == 2 && key <= 65535) return true;
			else if (keyLength == 3 && key <= 1677215) return true;
			else if (keyLength == 4 && key <= int.MaxValue) return true;

			return false;
		}

		private void ExpandKeySize()
		{
			if (keyLength == 4) { throw new OverflowException($"Your file required more than ${compressionDictionary.Count} keys. We've ran out of key space!"); }

			Data = initialData;
			UpdateUTF8Cache();
			compressionDictionary.Clear();
			currentKey = 0;
			keyLength++;
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

		private int GetSubstitutionSavings(int patternLength, int occurrences)
		{
			int substitutionSavings = (patternLength * occurrences) + (occurrences * keyLength);
			int dictionaryCost = keyLength + DictionaryValueLengthSize + patternLength;

			return substitutionSavings - dictionaryCost;
		}

		private void UpdateUTF8Cache()
		{
			utf8Cache = Encoding.UTF8.GetBytes(Data);
		}
	}
}
