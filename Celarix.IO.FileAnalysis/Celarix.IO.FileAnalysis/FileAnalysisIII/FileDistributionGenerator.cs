using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions;
using Celarix.IO.FileAnalysis.Utilities;
using NLog;
using LongFile = Pri.LongPath.File;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public class FileDistributionGenerator
	{
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public unsafe IFileDistribution[] GenerateFileDistributions(string filePath)
		{
			logger.Info("Generating file distributions for {0}", filePath);
			using var reader = new BinaryReader(LongFile.OpenRead(filePath));
			var buffer = new byte[3072];
			int read;
			
			var totalRead = 0L;
			var nextLoggingAmount = 1_048_576L;
			var advancedProgress = new AdvancedProgress(new FileInfo(filePath).Length, DateTimeOffset.Now);
			
			var oneBitDistribution = new OneBitDistribution();
			var twoBitDistribution = new TwoBitDistribution();
			var fourBitDistribution = new FourBitDistribution();
			var eightBitDistribution = new EightBitDistribution();
			var sixteenBitDistribution = new SixteenBitDistribution();
			var twentyFourBitDistribution = new TwentyFourBitDistribution();
			var thirtyTwoBitDistribution = new ThirtyTwoBitDistribution();
			var sixtyFourBitDistribution = new SixtyFourBitDistribution();
			var oneHundredTwentyEightBitDistribution = new OneHundredTwentyEightBitDistribution();
			
			while ((read = reader.Read(buffer)) > 0)
			{
				totalRead += read;
				if (totalRead >= nextLoggingAmount)
				{
					advancedProgress.CurrentAmount = totalRead;
					logger.Info(advancedProgress.ToString());
					nextLoggingAmount += 1_048_576L;
				}
				
				if (read % 48 != 0)
				{
					// Fill out the gap between read and the next 48-byte boundary with zeroes.
					var padding = 48 - (read % 48);
					for (var i = 0; i < padding; i++)
					{
						buffer[read + i] = 0;
					}
					// Update read to include the padding.
					read += padding;
				}
				
				var span = new ReadOnlySpan<byte>(buffer, 0, read);
				var uint64Span = MemoryMarshal.Cast<byte, ulong>(span);
				var nybbleDistributionStart = &fourBitDistribution.Nybble0000Count;
				var twoBitDistributionStart = &twoBitDistribution.BitPair00Count;

				// Twenty-four-bit distributions
				for (int i = 0; i < uint64Span.Length; i += 3)
				{
					var high24 = uint64Span[i];
					var mid24 = uint64Span[i + 1];
					var low24 = uint64Span[i + 2];

					// 8 bytes * 3 = 24 bytes, which is 192 bits. That's 8 24-bit values.
					// 01234567 01234567 01234567
					// 00011122 23334445 55666777

					twentyFourBitDistribution.AddEight((int)(high24 >> 40),
						(int)(high24 >> 16) & 0xffffff,
						(int)(high24 & 0xffff) | (int)(mid24 >> 56),
						(int)(mid24 >> 32) & 0xffffff,
						(int)(mid24 >> 8) & 0xffffff,
						(int)(mid24 & 0xff) | (int)(low24 >> 48),
						(int)(low24 >> 24) & 0xffffff,
						(int)(low24 & 0xffffff));
				}

				// One-hundred-twenty-eight-bit distribution
				for (int i = 0; i < uint64Span.Length; i += 2)
				{
					var high64 = uint64Span[i];
					var low64 = uint64Span[i + 1];
					oneHundredTwentyEightBitDistribution.Add(high64, low64);
				}

				foreach (var eightBytes in uint64Span)
				{
					// Sixty-four-bit distributions
					sixtyFourBitDistribution.Add(eightBytes);
					
					// Thirty-two-bit distributions
					var uintSource = eightBytes;
					for (var i = 0; i < 2; i++)
					{
						var uintValue = (uint)uintSource;
						thirtyTwoBitDistribution.Add(uintValue);
						uintSource >>= 32;
					}
					
					// Sixteen-bit distributions
					var shortSource = eightBytes;
					fixed (long* sixteenBitDistributionStart = &sixteenBitDistribution.counts[0])
					{
						for (var i = 0; i < 4; i++)
						{
							var shortValue = shortSource & 0xffff;
							*(sixteenBitDistributionStart + (int)shortValue) += 1;
							shortSource >>= 16;
						}
					}
					
					// Eight-bit distributions
					var byteSource = eightBytes;
					fixed (long* eightBitDistributionStart = &eightBitDistribution.counts[0])
					{
						for (var i = 0; i < 8; i++)
						{
							var byteValue = byteSource & 0xff;
							*(eightBitDistributionStart + (int)byteValue) += 1;
							byteSource >>= 8;
						}
					}
					
					// Four-bit distributions
					var nybbleSource = eightBytes;
					for (var i = 0; i < 16; i++)
					{
						var nybble = nybbleSource & 0b1111;
						*(nybbleDistributionStart + (int)nybble) += 1;
						nybbleSource >>= 4;
					}
					
					// Two-bit distributions
					var bitPairSource = eightBytes;
					for (var i = 0; i < 32; i++)
					{
						var bitPair = bitPairSource & 0b11;
						*(twoBitDistributionStart + (int)bitPair) += 1;
						bitPairSource >>= 2;
					}
					
					// One-bit distribution
					var bit1Count = ulong.PopCount(eightBytes);
					oneBitDistribution.Bit1Count += (long)bit1Count;
					oneBitDistribution.Bit0Count += 64 - (long)bit1Count;
				}
			}
			
			return
			[
				oneBitDistribution,
				twoBitDistribution,
				fourBitDistribution,
				eightBitDistribution,
				sixteenBitDistribution,
				twentyFourBitDistribution,
				thirtyTwoBitDistribution,
				sixtyFourBitDistribution,
				oneHundredTwentyEightBitDistribution
			];
		}
	}
}
