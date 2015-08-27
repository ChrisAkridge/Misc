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
					Console.WriteLine($"Converting input file ({GetFriendlyFileSize(inputFileLength)}) into octal string ({GetFriendlyFileSize((long)(inputFileLength * 1.3333))}");
					WriteText(outputFilePath, BaseWriter.BytesToOctalString(File.ReadAllBytes(inputFilePath)));
					break;
				case "-d":
					Console.WriteLine($"Converting input file ({GetFriendlyFileSize(inputFileLength)} into decimal string");
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
				case "-fc":
					FastCompressor fastCompressor = new FastCompressor(File.ReadAllText(inputFilePath), 16);
					fastCompressor.Compress();
					fastCompressor.WriteToDisk(outputFilePath);
					break;
				default:
					break;
			}
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
