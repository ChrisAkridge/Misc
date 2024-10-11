using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileUtility.Commands;

namespace Celarix.IO.FileUtility.Logic
{
	internal static class LinkListProcessor
	{
		public static void ProcessLinkList(ProcessLinkListCommand options)
		{
			var links = File.ReadAllLines(options.LinkListPath!)
				.Where(l => !string.IsNullOrWhiteSpace(l));

			var imageLinks = new List<string>();
			var videoLinks = new List<string>();
			var otherLinks = new List<string>();

			foreach (var link in links)
			{
				if (HasComment(link))
				{
					otherLinks.Add(link);
				}
				else if (IsPreviewRedditImageLink(link))
				{
					imageLinks.Add(ProcessPreviewRedditImageLink(link));
				}
				else if (IsRedditImageLink(link))
				{
					imageLinks.Add(RemoveQueryString(link));
				}
				else if (IsPictureLink(link))
				{
					imageLinks.Add(link);
				}
				else if (IsKnowYourMemeLink(link))
				{
					imageLinks.Add(link);
				}
				else if (IsRedditVideoLink(link))
				{
					videoLinks.Add(RemoveQueryString(link));
				}
				else
				{
					otherLinks.Add(link);
				}
			}

			var outputLinkList = new List<string>();
			outputLinkList.AddRange(imageLinks);
			outputLinkList.Add("");
			outputLinkList.AddRange(videoLinks);
			outputLinkList.Add("");
			outputLinkList.AddRange(otherLinks);
			File.WriteAllLines(options.LinkListPath!, outputLinkList);
		}
		
		// Link type determiners
		private static bool HasComment(string link) => link.Contains(' ') && link.EndsWith(')');
		
		private static bool IsPreviewRedditImageLink(string link) =>
			link.Contains("preview.redd.it")
			&& !link.Contains("external-preview");

		private static bool IsRedditImageLink(string link) => link.Contains("i.redd.it");
		
		private static bool IsRedditVideoLink(string link) => link.Contains("v.redd.it");

		private static bool IsPictureLink(string link)
		{
			var linkWithoutQueryString = RemoveQueryString(link);
			var extension = Path.GetExtension(linkWithoutQueryString);
			return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".mp4" or ".webp";
		}

		private static bool IsKnowYourMemeLink(string link) => link.Contains("kym-cdn.com");
		
		// Link processors
		private static string RemoveQueryString(string link) => link.Split('?')[0];
		
		private static string ProcessPreviewRedditImageLink(string link) =>
			RemoveQueryString(link.Replace("preview.redd.it", "i.redd.it"));
	}
}
