using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompression
{
	public sealed class BinaryRLECompressor
	{
		public BinaryRLECompressionStage Stage { get; private set; }

		public byte[] InputFile { get; private set; }
		public byte[] Binary { get; private set; }
		public List<BinaryRun> Runs { get; private set; }
		public byte[] Compressed { get; private set; }

		public string BinaryString
		{
			get
			{
				return ToBinaryString(InputFile);
			}
		}

		public string RunsString
		{
			get
			{
				StringBuilder result = new StringBuilder();

				foreach (BinaryRun run in Runs)
				{
					result.Append($"{{{((run.BitValue) ? '1' : '0')}:{run.Length}}}");
				}

				return result.ToString();
			}
		}

		public string CompressedString
		{
			get
			{
				return ToBinaryString(Compressed);
			}
		}

		public BinaryRLECompressor(string text)
		{
			if (string.IsNullOrEmpty(text)) throw new ArgumentException(nameof(text), "The provided text was empty.");

			InputFile = Encoding.UTF8.GetBytes(text);
			Stage = BinaryRLECompressionStage.Input;
		}
		public BinaryRLECompressor(byte[] file)
		{
			if (file == null || file.Length == 0) throw new ArgumentException(nameof(file), "The provided file had no bytes.");

			InputFile = file;
			Stage = BinaryRLECompressionStage.Input;
		}

		public static string ToBinaryString(byte[] bytes)
		{
			StringBuilder result = new StringBuilder(bytes.Length * 8);

			foreach (byte b in bytes)
			{
				result.Append(((b & 0x80) != 0) ? '1' : '0');
				result.Append(((b & 0x40) != 0) ? '1' : '0');
				result.Append(((b & 0x20) != 0) ? '1' : '0');
				result.Append(((b & 0x10) != 0) ? '1' : '0');
				result.Append(((b & 0x08) != 0) ? '1' : '0');
				result.Append(((b & 0x04) != 0) ? '1' : '0');
				result.Append(((b & 0x02) != 0) ? '1' : '0');
				result.Append(((b & 0x01) != 0) ? '1' : '0');
			}

			return result.ToString();
		}

		public void ToBinary()
		{
			Binary = InputFile;
			Stage = BinaryRLECompressionStage.ConvertToBinary;
		}

		public void ToBinaryRuns()
		{
			Runs = new List<BinaryRun>();
			string binaryString = BinaryString;

			bool currentBit = false;
			int currentLength = 0;

			foreach (char c in binaryString)
			{
				bool bit = (c == '1') ? true : false;
				if (bit != currentBit)
				{
					Runs.Add(new BinaryRun(currentBit, (byte)currentLength));
					currentBit = bit;
					currentLength = 0;
				}
				else
				{
					if (currentLength == 127)
					{
						Runs.Add(new BinaryRun(currentBit, 127));
						currentLength = 1;
					}
					else
					{
						currentLength++;
					}
				}
			}

			Stage = BinaryRLECompressionStage.BinaryRunSequences;
		}

		public void ToCompressedBinary()
		{
			List<byte> result = new List<byte>();

			foreach (BinaryRun run in Runs)
			{
				byte b = 0;
				b |= (byte)((run.BitValue) ? 0x80 : 0x00);
				b += run.Length;

				result.Add(b);
			}

			Compressed = result.ToArray();

			Stage = BinaryRLECompressionStage.RLEEncodedBytes;
		}

		public void NextCompressionStep()
		{
			switch (Stage)
			{
				case BinaryRLECompressionStage.Input:
					ToBinary();
					break;
				case BinaryRLECompressionStage.ConvertToBinary:
					ToBinaryRuns();
					break;
				case BinaryRLECompressionStage.BinaryRunSequences:
					ToCompressedBinary();
					break;
				case BinaryRLECompressionStage.RLEEncodedBytes:
				default:
					break;
			}
		}
	}

	public struct BinaryRun
	{
		public bool BitValue { get; }
		public byte Length { get; }

		public BinaryRun(bool bitValue, byte length)
		{
			if (length >= 127) throw new ArgumentOutOfRangeException(nameof(length), $"A binary run cannot be longer than 127 bits. The provided length was {length} bits.");

			BitValue = bitValue;
			Length = length;
		}
	}

	public enum BinaryRLECompressionStage
	{
		Input,
		ConvertToBinary,
		BinaryRunSequences,
		RLEEncodedBytes
	}
}
