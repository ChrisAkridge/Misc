using System;
using System.IO;
using System.Buffers.Binary;

namespace Celarix.JustForFun.FootballSimulator.SummaryWriting
{
    // SPDX-License-Identifier: GPL-2.0
    internal static class MakeBootblock
    {
        private const int BootblockSize = 512;
        private const int QuadwordCount = 64;
        private const int ChecksumIndex = 63;
        private const int ChecksumOffset = ChecksumIndex * 8;
        private const int DiskLabelOffset = 64;
        private const int DiskLabelSize = 276; // Calculated from original C struct layout

        private static int _Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: mkbb <device> <lxboot>");
                return 1;
            }

            string devicePath = args[0];
            string lxbootPath = args[1];

            byte[] loaderBytes = new byte[BootblockSize];
            byte[] diskBytes = new byte[BootblockSize];

            try
            {
                // Read lxboot image
                using (var lxfs = new FileStream(lxbootPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int read = ReadExactly(lxfs, loaderBytes, 0, BootblockSize);
                    if (read != BootblockSize)
                    {
                        Console.Error.WriteLine($"lxboot read: expected {BootblockSize}, got {read}");
                        return 2;
                    }
                }

                // Open device for read/write
                using (var devFs = new FileStream(devicePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    int readDev = ReadExactly(devFs, diskBytes, 0, BootblockSize);
                    if (readDev != BootblockSize)
                    {
                        Console.Error.WriteLine($"bootblock read: expected {BootblockSize}, got {readDev}");
                        return 3;
                    }

                    // Swap disklabel from disk into loader image
                    Array.Copy(diskBytes, DiskLabelOffset, loaderBytes, DiskLabelOffset, DiskLabelSize);

                    // Zero checksum field before calculation
                    for (int i = 0; i < 8; i++)
                    {
                        loaderBytes[ChecksumOffset + i] = 0;
                    }

                    // Calculate checksum as sum of first 63 8-byte little-endian quadwords
                    ulong checksum = 0;
                    for (int i = 0; i < ChecksumIndex; i++)
                    {
                        int offset = i * 8;
                        // Read as little-endian
                        ulong value = BinaryPrimitives.ReadUInt64LittleEndian(loaderBytes.AsSpan(offset, 8));
                        checksum = unchecked(checksum + value);
                    }

                    // Write checksum back (little-endian)
                    BinaryPrimitives.WriteUInt64LittleEndian(loaderBytes.AsSpan(ChecksumOffset, 8), checksum);

                    // Write modified bootblock back to device
                    devFs.Seek(0, SeekOrigin.Begin);
                    devFs.Write(loaderBytes, 0, BootblockSize);
                    devFs.Flush();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"Access error: {ex.Message}");
                return 4;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"File not found: {ex.Message}");
                return 5;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"I/O error: {ex.Message}");
                return 6;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                return 99;
            }

            return 0;
        }

        private static int ReadExactly(Stream stream, byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = stream.Read(buffer, offset + totalRead, count - totalRead);
                if (read <= 0) break;
                totalRead += read;
            }
            return totalRead;
        }
    }
}
