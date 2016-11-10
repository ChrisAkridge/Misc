using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TournamentOfPictures
{
	public sealed class QuickSorter
	{
		// https://gist.github.com/lbsong/6833729#gistcomment-1853006
		private QuickSortForm form;
		private List<string> pictures;
		private List<Tuple<string, int>> forcedTransitivitySort = new List<Tuple<string, int>>();

		public QuickSorter(QuickSortForm form, IEnumerable<string> pictures)
		{
			this.form = form;
			this.pictures = pictures.ToList();
		}

		//private void InsertInForcedTransitivity(string item)
		//{
			
		//}

		//private int Compare(string item1, string item2)
		//{
		//	var picturesInForcedTransitivity = forcedTransitivitySort.Select(p => p.Item1);
		//	bool item1Contained = picturesInForcedTransitivity.Contains(item1);
		//	bool item2Contained = picturesInForcedTransitivity.Contains(item2);

		//	if (item1Contained && item2Contained)
		//	{
		//		var item1Tuple = forcedTransitivitySort.First(p => p.Item1 == item1);
		//		var item2Tuple = forcedTransitivitySort.First(p => p.Item1 == item2);
		//		return item1Tuple.Item2.CompareTo(item2Tuple.Item2);
		//	}
		//	else if (item1Contained && !item2Contained)
		//	{
		//		var item1Tuple = forcedTransitivitySort.First(p => p.Item1 == item1);

		//	}
		//}

		public List<string> Start()
		{
			return QuickSort(pictures);
		}

		private List<string> QuickSort(List<string> pictures)
		{
			if (pictures.Count < 2) { return pictures; }

			Random random = new Random();
			int pivot = random.Next(0, pictures.Count);
			string picture = pictures[pivot];
			List<string> lesser = new List<string>();
			List<string> greater = new List<string>();
			for (int i = 0; i < pictures.Count; i++)
			{
				if (i != pivot)
				{
					if (UserCompare(pictures[i], picture) < 0)
					{
						lesser.Add(pictures[i]);
					}
					else
					{
						greater.Add(pictures[i]);
					}
				}
			}

			var merged = QuickSort(lesser);
			merged.Add(picture);
			merged.AddRange(QuickSort(greater));
			return merged;
		}

		private int UserCompare(string item1, string item2)
		{
			AutoResetEvent resetEvent = new AutoResetEvent(false);
			form.PrepareComparison(item1, item2, resetEvent);
			resetEvent.WaitOne();

			if (form.CompareResult == 2) { return 1; }
			else { return -1; }
		}
	}
}
