using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropSave
{
	public sealed class FileSaver
	{
		private const string BasePath = @"C:\Users\Chris\Documents\Files\Pictures\Pictures";
		private static readonly string[] subfolders = new string[]
		{@"Alleret Group\Ascending Series", @"Alleret Group\Beneficial Series", @"Alleret Group\Celestial Series", 
		 @"Alleret Group\Dulcet Series", @"Alleret Group\Effervescent Series", @"Alleret Group\Felicitous Series", 
		 @"Belcival Group\Glistering Series", @"Belcival Group\Halcyon Series", @"Belcival Group\Illumination Series", 
		 @"Belcival Group\Jade Series", @"Belcival Group\Kinematic Series",@"Belcival Group\Lilting Series", 
		 @"Celaron Group\Mellifluous Series", @"Celaron Group\Numerous Series", @"Celaron Group\Omnigenerous Series", 
		 @"Celaron Group\Petrichoric Series", @"Celaron Group\Quadratic Series", @"Celaron Group\Redolence Series", 
		 @"Doralin Group\Safer Series", @"Doralin Group\Transcendent Series", @"Doralin Group\Universal Series", 
		 @"Doralin Group\Vivarium Series", @"Doralin Group\Withering Series", @"Doralin Group\Yearling Series", };
		private static readonly char[] picturePrefixes = new char[]
		{ 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
		  'v', 'w', 'y' };

		private int currentSubfolderIndex = 0;
		private int currentPictureNumber = 0;

		public FileSaver()
		{
			// Determine the last saved picture in the folder.
			int i;
			int topFolderPictureCount = 0;
			for (i = 0; i < subfolders.Length; i++)
			{
				int pictureCount = this.GetPictureCount(i);

				if (pictureCount < 250)
				{
					// We've found the "top" folder.
					topFolderPictureCount = pictureCount;
					break;
				}
			}

			this.currentSubfolderIndex = i;
			this.currentPictureNumber = topFolderPictureCount;
		}

		public string GetNextFilePath(string extension)
		{
			if (this.currentPictureNumber == 250)
			{
				this.currentSubfolderIndex++;
				this.currentPictureNumber = 1;
			}
			else
			{
				this.currentPictureNumber++;
			}

			if (this.currentSubfolderIndex >= subfolders.Length - 1)
			{
				throw new Exception("Picture folder is full.");
			}

			string path = string.Concat(BasePath, "\\", subfolders[this.currentSubfolderIndex], "\\", picturePrefixes[this.currentSubfolderIndex], string.Format("{0:D3}", this.currentPictureNumber), extension);

			return path;
		}

		public string PeekNextFile()
		{
			int subfolderIndex = this.currentSubfolderIndex;
			int pictureNumber = this.currentPictureNumber;

			if (this.currentPictureNumber == 250)
			{
				subfolderIndex++;
				pictureNumber = 1;
			}
			else
			{
				pictureNumber++;
			}

			if (subfolderIndex > subfolders.Length - 1)
			{
				return "End of folders.";
			}

			return string.Format("{0}{1:D3}", picturePrefixes[subfolderIndex], pictureNumber);
		}

		private int GetPictureCount(int subfolderIndex)
		{
			if (subfolderIndex < 0 || subfolderIndex >= FileSaver.subfolders.Length)
			{
				throw new ArgumentOutOfRangeException();
			}

			string folderPath = string.Concat(BasePath, "\\", FileSaver.subfolders[subfolderIndex], "\\");

			for (int i = 1; i <= 250; i++)
			{
				string fileNamePattern = string.Format("{0}{1:D3}.*", picturePrefixes[subfolderIndex], i);
				var matchingFiles = Directory.GetFiles(folderPath, fileNamePattern, SearchOption.TopDirectoryOnly);

				if (matchingFiles.Length == 0)
				{
					// we've reached passed the last picture in the folder
					return i - 1;
				}
			}

			// the folder is full.
			return 250;
		}
	}
}
