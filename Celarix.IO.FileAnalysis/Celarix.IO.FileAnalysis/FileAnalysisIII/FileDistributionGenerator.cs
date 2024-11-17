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
				var uint128Span = MemoryMarshal.Cast<byte, UInt128>(span);
				var nybbleDistributionStart = &fourBitDistribution.Nybble0000Count;
				var twoBitDistributionStart = &twoBitDistribution.BitPair00Count;

				// Twenty-four-bit distributions
				for (int i = 0; i < uint128Span.Length; i += 3)
				{
					var high24 = uint128Span[i];
					var mid24 = uint128Span[i + 1];
					var low24 = uint128Span[i + 2];

					// 16 bytes * 3 = 48 bytes, which is 384 bits. That's 16 24-bit values.
					// 0123456789ABCDEF 0123456789ABCDEF 0123456789ABCDEF
					// 0001112223334445 55666777888999AA ABBBCCCDDDEEEFFF

					twentyFourBitDistribution.AddSixteen((int)(high24 >> 104),
						(int)(high24 >> 80) & 0xffffff,
						(int)(high24 >> 56) & 0xffffff,
						(int)(high24 >> 32) & 0xffffff,
						(int)(high24 >> 8) & 0xffffff,
						(int)(((high24 & 0xff) << 16) | (mid24 >> 112)),
						(int)(mid24 >> 88) & 0xffffff,
						(int)(mid24 >> 64) & 0xffffff,
						(int)(mid24 >> 40) & 0xffffff,
						(int)(mid24 >> 16) & 0xffffff,
						(int)(((mid24 & 0xffff) << 8) | (low24 >> 120)),
						(int)(low24 >> 96) & 0xffffff,
						(int)(low24 >> 72) & 0xffffff,
						(int)(low24 >> 48) & 0xffffff,
						(int)(low24 >> 24) & 0xffffff,
						(int)low24 & 0xffffff);
				}

				foreach (var sixteenBytes in uint128Span)
				{
					// One-hundred-twenty-eight-bit distribution
					oneHundredTwentyEightBitDistribution.Add(sixteenBytes);
					
					// Sixty-four-bit distributions
					sixtyFourBitDistribution.Add((ulong)(sixteenBytes >> 64));
					sixtyFourBitDistribution.Add((ulong)sixteenBytes);
					
					// Thirty-two-bit distributions
					var uintSource = sixteenBytes;
					for (var i = 0; i < 4; i++)
					{
						var uintValue = (uint)uintSource;
						thirtyTwoBitDistribution.Add(uintValue);
						uintSource >>= 32;
					}
					
					// Sixteen-bit distributions
					var shortSource = sixteenBytes;
					fixed (long* sixteenBitDistributionStart = &sixteenBitDistribution.counts[0])
					{
						for (var i = 0; i < 8; i++)
						{
							var shortValue = shortSource & 0xffff;
							*(sixteenBitDistributionStart + (int)shortValue) += 1;
							shortSource >>= 16;
						}
					}
					
					// Eight-bit distributions
					var byteSource = sixteenBytes;
					fixed (long* eightBitDistributionStart = &eightBitDistribution.counts[0])
					{
						for (var i = 0; i < 16; i++)
						{
							var byteValue = byteSource & 0xff;
							*(eightBitDistributionStart + (int)byteValue) += 1;
							byteSource >>= 8;
						}
					}
					
					// Four-bit distributions
					var nybbleSource = sixteenBytes;
					for (var i = 0; i < 32; i++)
					{
						var nybble = nybbleSource & 0b1111;
						*(nybbleDistributionStart + (int)nybble) += 1;
						nybbleSource >>= 4;
					}
					
					// Two-bit distributions
					var bitPairSource = sixteenBytes;
					for (var i = 0; i < 64; i++)
					{
						var bitPair = bitPairSource & 0b11;
						*(twoBitDistributionStart + (int)bitPair) += 1;
						bitPairSource >>= 2;
					}
					
					// One-bit distribution
					var bit1Count = UInt128.PopCount(sixteenBytes);
					oneBitDistribution.Bit1Count += (long)bit1Count;
					oneBitDistribution.Bit0Count += 128 - (long)bit1Count;
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
