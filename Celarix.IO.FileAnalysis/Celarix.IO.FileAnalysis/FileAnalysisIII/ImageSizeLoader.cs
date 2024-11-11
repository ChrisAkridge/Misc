using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class ImageSizeLoader
	{
		public bool TryGetSize(string path, out Size size)
		{
			using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new BinaryReader(stream);
			
			var header = reader.ReadBytes(4);
			if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46)
			{
				return TryDecodeWebP(reader, out size);
			}
			
			// ughhh I don't feel like publishing a new NuGet package just for this
			// just reflect out the static method Celarix.Imaging.Packing.ImageSizeLoader.TryGetSize(string, out SixLabors.ImageSharp.Size)
			// and call it here
			size = default;
			var imageSizeLoaderType = typeof(Imaging.LibraryConfiguration).Assembly.GetType("Celarix.Imaging.Packing.ImageSizeLoader");
			var tryGetSizeMethod = imageSizeLoaderType.GetMethod("TryGetSize", [typeof(string), typeof(Size).MakeByRefType()]);
			var result = tryGetSizeMethod.Invoke(null, [path, size]);
			
			if ((bool)result)
			{
				return true;
			}
			
			size = default;
			return false;
		}
		
		private static bool TryDecodeWebP(BinaryReader reader, out Size size)
		{
			try
			{
				// The reader should be 4 bytes into the file. The next 4 bytes are the size of the file.
				// so we can skip them.
				reader.BaseStream.Seek(4, SeekOrigin.Current);

				// Read the next four bytes and check if they are "WEBP" (0x57, 0x45, 0x42, 0x50)
				var webpHeader = reader.ReadBytes(4);

				if (webpHeader[0] != 0x57 || webpHeader[1] != 0x45 || webpHeader[2] != 0x42 || webpHeader[3] != 0x50)
				{
					size = default;

					return false;
				}

				// Read the next four bytes as a string. There are many things it could be, all representing
				// different internal formats of the WebP file. Code taken from SixLabors.ImageSharp, specifically
				// https://github.com/SixLabors/ImageSharp/blob/main/src/ImageSharp/Formats/Webp/WebpDecoderCore.cs#L174
				// under the Apache 2.0 license.
				var webpInternalHeader = reader.ReadBytes(4);
				var internalHeaderString = Encoding.ASCII.GetString(webpInternalHeader);

				if (internalHeaderString == "VP8 ")
				{
					// "VP8 " is the simple lossy file format. Skip the first four bytes, which is the remaining
					// chunk size...
					reader.BaseStream.Seek(4, SeekOrigin.Current);
					// Then skip 3 more bytes, which is the frame tag...
					reader.BaseStream.Seek(3, SeekOrigin.Current);
					// Then 3 more for the VP8 magic bytes...
					reader.BaseStream.Seek(3, SeekOrigin.Current);
					// Then read a UInt16...
					var tmp = reader.ReadUInt16();
					var width = tmp & 0x3fff;
					// Then another...
					tmp = reader.ReadUInt16();
					var height = tmp & 0x3fff;
					
					size = new Size(width, height);
					return true;
				}

				if (internalHeaderString == "VP8L")
				{
					// "VP8L" is the lossless file format. Skip four bytes, which should be the chunk size...
					reader.BaseStream.Seek(4, SeekOrigin.Current);
					// Then skip one byte, which should be the signature byte (0x2f)...
					reader.BaseStream.Seek(1, SeekOrigin.Current);
					
					// Then read 4 bytes.
					var tmp = reader.ReadUInt32();
					
					// This contains 28 bits, which is 14 bits for width and 14 bits for height.
					var width = (int)(tmp >> 18);
					var height = (int)(tmp >> 4) & 0x3fff;
					
					size = new Size(width + 1, height + 1);
					return true;
				}

				if (internalHeaderString == "VP8X")
				{
					// "VP8X" is the extended file format. Skip four bytes, which should be the chunk size...
					reader.BaseStream.Seek(4, SeekOrigin.Current);
					// Then skip one byte, which should be the image features byte...
					reader.BaseStream.Seek(1, SeekOrigin.Current);
					// Then three more, which are unused...
					reader.BaseStream.Seek(3, SeekOrigin.Current);
					
					// Then, read 3 bytes for the width...
					var widthBytes = reader.ReadBytes(3);
					var width = widthBytes[0] | (widthBytes[1] << 8) | (widthBytes[2] << 16);
					var heightBytes = reader.ReadBytes(3);
					var height = heightBytes[0] | (heightBytes[1] << 8) | (heightBytes[2] << 16);
					
					size = new Size(width + 1, height + 1);
					return true;
				}
				
				size = default;
				return false;
			}
			catch
			{
				size = default;
				return false;
			}
		}
	}
}
