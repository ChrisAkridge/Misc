using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTools
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			// Usage: FileTools -i "path//to/file.ext"
			//	-b Write a file at "path//to//file_bin.ext" containing the file as a sequence of 0s and 1s
			//	-o Write a file containing the file as a sequence of octal digits (8 octal digits == 6 bytes)
			//	-d Write a file containing the file as a single, very large decimal number
			//  -h Write a file containing the file as a sequence of hex digits (4 bits per digit)
			//	-64 Write a file containing the file as a sequence of Base64 digits
			//	-c Compress the file

			// -c1 Count the number of distinct bits in the file
			// -c2 Count the number of distinct bit pairs in the file
			// -c4 Count the number of distinct nybbles in the file
			// -c8 Count the number of distinct bytes in the file
			// -c16 Count the number of distinct 16-bit integers in the file
			// -c32 Count the number of distinct 32-bit integers in the file
			// -c64 Count the number of distinct 64-bit integers in the file

			if (args[0] == "-g")
			{
				new CompressorGUI().ShowDialog();
			}

			if (args.Length != 5 || args[0] != "-i" || args[2] != "-o")
			{
				WriteUsage();
			}

			string inputFilePath = args[1];
			string outputFilePath = args[3];
			string option = args[4].ToLowerInvariant();

			if (!File.Exists(inputFilePath))
			{
				Console.WriteLine($"The file at {inputFilePath} does not exist.");
				Console.ReadKey(intercept: true);
				Environment.Exit(2);
			}

			long inputFileLength = new FileInfo(inputFilePath).Length;

			switch (option)
			{
				case "-b":
					Console.WriteLine($"Converting input file ({GetFriendlyFileSize(inputFileLength)}) into binary string ({GetFriendlyFileSize(inputFileLength * 8L)})");
					WriteText(outputFilePath, BaseWriter.BytesToBinaryString(File.ReadAllBytes(inputFilePath)));
					break;
				case "-o":
					Console.WriteLine($"Converting input file ({GetFriendlyFileSize(inputFileLength)}) into octal string ({GetFriendlyFileSize((long)(inputFileLength * 1.3333))})");
					WriteText(outputFilePath, BaseWriter.BytesToOctalString(File.ReadAllBytes(inputFilePath)));
					break;
				case "-d":
					Console.WriteLine($"Converting input file ({GetFriendlyFileSize(inputFileLength)}) into decimal string");
					WriteText(outputFilePath, BaseWriter.BytesToDecimalString(File.ReadAllBytes(inputFilePath)));
					break;
				case "-h":
					Console.WriteLine($"Converting input file ({GetFriendlyFileSize(inputFileLength)} into hexadecimal string ({GetFriendlyFileSize(inputFileLength * 2L)})");
					WriteText(outputFilePath, BaseWriter.BytesToHexString(File.ReadAllBytes(inputFilePath)));
					break;
				case "-ho":
					WriteText(outputFilePath, BaseWriter.BytesToOffsetHexString(File.ReadAllBytes(inputFilePath)));
					break;
				case "-64":
					WriteText(outputFilePath, BaseWriter.BytesToBase64String(File.ReadAllBytes(inputFilePath)));
					break;
				case "-c":
					StringCompressor compressor = new StringCompressor(File.ReadAllText(inputFilePath), 4);
					compressor.Compress();
					compressor.WriteToDisk(outputFilePath);
					break;
				case "-lzf":
					LZF lzf = new LZF();
					byte[] buffer = new byte[inputFileLength + 1024];
					int outputLength = lzf.Compress(File.ReadAllBytes(inputFilePath), (int)inputFileLength, buffer, (int)(inputFileLength + 1024));
					byte[] output = new byte[outputLength];
					Array.Copy(buffer, output, outputLength);
					File.WriteAllBytes(outputFilePath, output);
					break;
				case "-fc":
					FastCompressor fastCompressor = new FastCompressor(File.ReadAllText(inputFilePath), 16);
					fastCompressor.Compress();
					fastCompressor.WriteToDisk(outputFilePath);
					break;
				case "-c1":
					var bitCount = SequenceCounter.CountBits(File.ReadAllBytes(inputFilePath));
					float clearBitPercentage = bitCount.Item1 / (float)(bitCount.Item1 + bitCount.Item2) * 100f;
					float setBitPercentage = 100f - clearBitPercentage;
					Console.WriteLine("Clear bits: {0} ({1:F2}%)", bitCount.Item1, clearBitPercentage);
					Console.WriteLine("Set bits: {0} ({1:F2}%)", bitCount.Item2, setBitPercentage);
					Console.ReadKey(intercept: true);
					break;
				case "-c2":
					var bitPairCount = SequenceCounter.CountBitPairs(File.ReadAllBytes(inputFilePath));
					long sumc2 = bitPairCount.Sum();
					for (int i = 0; i <= 3; i++)
					{
						float percentage = bitPairCount[i] / (float)sumc2 * 100f;
						Console.WriteLine("Bit pair {0}: {1} ({2:F2}%)", Convert.ToString(i, 2).PadLeft(2, '0'), bitPairCount[i], percentage);
					}
					Console.ReadKey(intercept: true);
					break;
				case "-c4":
					var nybbleCount = SequenceCounter.CountNybbles(File.ReadAllBytes(inputFilePath));
					long sumc4 = nybbleCount.Sum();
					for (int i = 0; i <= 15; i++)
					{
						float percentage = nybbleCount[i] / (float)sumc4 * 100f;
						Console.WriteLine("Nybble {0}: {1} ({2:F2}%)", Convert.ToString(i, 16), nybbleCount[i], percentage);
					}
					Console.ReadKey(intercept: true);
					break;
				case "-c8":
					var byteCount = SequenceCounter.CountBytes(File.ReadAllBytes(inputFilePath));
					long sumc8 = byteCount.Sum();
					for (int i = 0; i <= 255; i++)
					{
						float percentage = byteCount[i] / (float)sumc8 * 100f;
						Console.WriteLine("Byte {0}: {1} ({2:F2}%)", Convert.ToString(i, 16).PadLeft(2, '0'), byteCount[i], percentage);
					}
					Console.ReadKey(intercept: true);
					break;
				case "-c16":
					var wordCount = SequenceCounter.CountWords(File.ReadAllBytes(inputFilePath)).OrderByDescending(kvp => kvp.Value);
					long sumc16 = wordCount.Sum(w => w.Value);
					if (File.Exists(outputFilePath)) { File.Delete(outputFilePath); }

					using (var writer = File.CreateText(outputFilePath))
					{
						foreach (var word in wordCount)
						{
							float percentage = word.Value / (float)sumc16 * 100f;
							writer.WriteLine($"Word {Convert.ToString(word.Key, 16).PadLeft(4, '0')}: {word.Value}, ({percentage:F6}%)");
						}
					}
					break;
				case "-c32":
					var dwordCount = SequenceCounter.CountDWords(File.ReadAllBytes(inputFilePath)).OrderByDescending(kvp => kvp.Value);
					long sumc32 = dwordCount.Sum(w => w.Value);
					if (File.Exists(outputFilePath)) { File.Delete(outputFilePath); }

					using (var writer = File.CreateText(outputFilePath))
					{
						foreach (var word in dwordCount)
						{
							float percentage = word.Value / (float)sumc32 * 100f;
							writer.WriteLine($"DWord {Convert.ToString(word.Key, 16).PadLeft(8, '0')}: {word.Value}, ({percentage:F6}%)");
						}
					}
					break;
				case "-c64":
					var qwordCount = SequenceCounter.CountQWords(File.ReadAllBytes(inputFilePath)).OrderByDescending(kvp => kvp.Value);
					long sumc64 = qwordCount.Sum(w => w.Value);
					if (File.Exists(outputFilePath)) { File.Delete(outputFilePath); }

					using (var writer = File.CreateText(outputFilePath))
					{
						foreach (var word in qwordCount)
						{
							float percentage = word.Value / (float)sumc64 * 100f;
							writer.WriteLine($"QWord {Convert.ToString(unchecked((long)word.Key), 16).PadLeft(16, '0')}: {word.Value}, ({percentage:F6}%)");
						}
					}
					break;
				default:
					break;
			}

			System.Threading.Thread.Sleep(1000);
		}

		private static void WriteUsage()
		{
			Console.WriteLine("Usage: FileTools -i [path\\to\\input] -o [path\\to\\output] -{b|o|d|h|64|c|c1|c2|c4|c8|c16|c32|c64}");
			Console.ReadKey(intercept: true);
			Environment.Exit(1);
		}

		private static void WriteText(string filePath, string text)
		{
			try
			{
				File.WriteAllText(filePath, text);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"The output file {filePath} has something wrong with it. Message: {ex.Message}");
				Console.ReadKey(intercept: true);
				Environment.Exit(2);
			}
		}

		private static string GetFriendlyFileSize(long fileSize)
		{
			double floatingFileSize = fileSize;
			string[] suffixes = new string[] { " bytes", " KB", " MB", " GB", " TB" };
			int suffixIndex = 0;

			while (floatingFileSize > 1024d && suffixIndex < suffixes.Length - 1)
			{
				floatingFileSize /= 1024d;
				suffixIndex++;
			}

			return string.Concat(string.Format("{0:F2}", floatingFileSize), suffixes[suffixIndex]);
		}
	}
}
