using System;
using System.IO;

namespace JarOfGoodThings
{
	/// <summary>
	/// Handles IO operations for this app.
	/// </summary>
	internal static class IO
	{
		/// <summary>
		/// The number of good things in the jar (lines in the file).
		/// </summary>
		public static int NumberOfGoodThings => File.ReadAllLines(GetJarFilePath()).Length;

		/// <summary>
		/// How long until the user is notified that the jar can be opened.
		/// The jar can be opened six hours before midnight on December 31 of any year.
		/// </summary>
		public static TimeSpan TimeUntilNotify
		{
			get
			{
				DateTime now = DateTime.Now;
				DateTime unlock = (new DateTime(now.Year + 1, 1, 1) - TimeSpan.FromHours(6));
				return (unlock - now);
			}
		}

		/// <summary>
		/// Creates this year's jar if it doesn't exist. The jar is created at
		/// %appdata%/JarOfGoodThings/YYYY.txt.
		/// </summary>
		public static void Initialize()
		 {
			// Verify that the jar exists, and if it doesn't, create it
			string jarPath = GetJarFilePath();
			string folderPath = Path.GetDirectoryName(jarPath);

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			if (!File.Exists(jarPath))
			{
				File.Create(jarPath);
			}
		}

		/// <summary>
		/// Gets the file path to this year's jar, which is %appdata%/JarOfGoodThings/YYYY.txt.
		/// </summary>
		/// <returns>The file path to this year's jar.</returns>
		internal static string GetJarFilePath()
		{
			int year = DateTime.Now.Year;
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			return Path.Combine(appDataPath, $"JarOfGoodThings\\{year}.txt");
		}

		/// <summary>
		/// Returns a value indicating whether the user can be notified that the jar is ready to be
		/// opened.
		/// </summary>
		/// <returns>True if <see cref="TimeUntilNotify"/> has passed, False if otherwise. </returns>
		public static bool CanNotifyUser()
		{
			return TimeUntilNotify < TimeSpan.FromHours(0);
		}

		/// <summary>
		/// Writes a good thing to the jar, prepended by the weekday and date.
		/// </summary>
		/// <param name="thing">The thing to write.</param>
		public static void WriteGoodThing(string thing)
		{
			string date = DateTime.Now.ToString("ddd yyyy-MM-dd");
			string fullEntry = $"{date}: {thing}";

			using (var file = new FileStream(GetJarFilePath(), FileMode.Append))
			using (var stream = new StreamWriter(file))
			{
				stream.WriteLine(fullEntry);
				stream.Close();
			}
		}
	}
}
