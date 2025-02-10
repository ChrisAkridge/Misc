using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.Data.Models
{
	public class FileRecord
	{
		[Key]
		public long FileId { get; set; }
		public long ParentFolderId { get; set; }
		public long Size { get; set; }
		public ulong MD5High { get; set; }
		public ulong MD5Low { get; set; }
		public string NameWithoutExtension { get; set; }
		public string Extension { get; set; }
		public string TRiDIdentifiedFileType { get; set; }
		public DateTimeOffset Created { get; set; }
		public DateTimeOffset Modified { get; set; }
		public int? ImageOrVideoWidth { get; set; }
		public int? ImageOrVideoHeight { get; set; }
		public int? ImageOrVideoFrameCount { get; set; }
		public decimal? ImageOrVideoDuration { get; set; }
		public int? AudioSampleRate { get; set; }
		public long? AudioSampleCount { get; set; }
		
		public virtual FolderRecord ParentFolder { get; set; }
		public virtual ICollection<FileDistributionSet> DistributionSets { get; set; }

		public string FullFilePath
		{
			get
			{
				var path = new StringBuilder();
				var currentFolder = ParentFolder;
				// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
				while (currentFolder != null)
				{
					path.Insert(0, currentFolder.Name + "/");
					currentFolder = currentFolder.ParentFolder;
				}
				path.Append(NameWithoutExtension);
				if (!string.IsNullOrEmpty(Extension))
				{
					path.Append("." + Extension);
				}
				return path.ToString();
			}
		}
		
		public int FullFilePathLength => FullFilePath.Length;

		public int FileDepth
		{
			get
			{
				var depth = 0;
				var currentFolder = ParentFolder;
				// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
				while (currentFolder != null)
				{
					depth++;
					currentFolder = currentFolder.ParentFolder;
				}

				return depth;
			}
		}
		
		public int FileNameLength => NameWithoutExtension.Length + 1 + Extension.Length;
		public int ParentFolderPathLength => FullFilePathLength - FileNameLength;
		
		public int TotalPixelCount =>
			(ImageOrVideoWidth ?? 0)
			* (ImageOrVideoHeight ?? 0)
			* (ImageOrVideoFrameCount ?? 0);
		
		public double BytesPerPixel =>
			(Size / (double)TotalPixelCount);
		
		public double BytesPerFrame =>
			(Size / (double)(ImageOrVideoFrameCount ?? 0));
		
		public double BytesPerAnimatedSecond =>
			Size / (double)(ImageOrVideoFrameCount ?? 0) / (double)(ImageOrVideoDuration ?? 0);
		
		public double BytesPerSample =>
			Size / (double)(AudioSampleCount ?? 0);
		
		public double BytesPerAudioSecond =>
			Size / (double)(AudioSampleCount ?? 0) / (AudioSampleRate ?? 0);

		public string? AspectRatio
		{
			get
			{
				if (ImageOrVideoWidth == null || ImageOrVideoHeight == null)
				{
					return null;
				}
				var gcd = GCD(ImageOrVideoWidth.Value, ImageOrVideoHeight.Value);
				return $"{ImageOrVideoWidth.Value / gcd}:{ImageOrVideoHeight.Value / gcd}";
			}
		}

		public TimeSpan MediaDuration
		{
			get
			{
				if (ImageOrVideoFrameCount is > 1)
				{
					return TimeSpan.FromSeconds(ImageOrVideoFrameCount.Value * (double)(ImageOrVideoDuration ?? 0));
				}
				
				if (AudioSampleCount != null && AudioSampleRate != null)
				{
					return TimeSpan.FromSeconds(AudioSampleCount.Value / (double)AudioSampleRate.Value);
				}
				
				return TimeSpan.Zero;
			}
		}
		
		private static int GCD(int a, int b)
		{
			while (b != 0)
			{
				var temp = b;
				b = a % b;
				a = temp;
			}
			return a;
		}
	}
}
