using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
	public static class PlaylistCreator
	{
		public static void CreatePlaylist(string playlistPath, List<string> files)
		{
			if (string.IsNullOrEmpty(playlistPath)) { throw new ArgumentNullException("The path of the playlist to create must not be null or empty."); }
			else if (files == null || !files.Any()) { throw new ArgumentException("The files for the playlist must have files."); }

			System.IO.File.WriteAllLines(playlistPath, files.ToArray());
		}
	}
}
