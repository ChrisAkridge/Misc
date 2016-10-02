using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
	public sealed class PictureList
	{
		private List<string> filePaths;

		public int Count => filePaths.Count;
		public string this[int index] => filePaths[index];

		public PictureList(IEnumerable<string> filePaths)
		{
			this.filePaths = filePaths.ToList();
		}
	}
}
