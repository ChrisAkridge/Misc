using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
	public sealed class QuantificationTournament<T> where T : class
	{
		private List<ScoredItem<T>> items;
		public int CurrentIndex { get; private set; } = 0;

		public T Current => items[CurrentIndex].Item;
		public int Count => items.Count;
		public QuantificationWinnerSelectedEventHandler<T> WinnerSelected;

		public QuantificationTournament(IEnumerable<T> items)
		{
			this.items = items.Select(i => new ScoredItem<T>(i)).ToList();
		}

		private void Shuffle()
		{
			Random random = new Random();
			int n = items.Count;

			while (n > 1)
			{
				n--;
				int k = random.Next(n + 1);
				ScoredItem<T> value = items[k];
				items[k] = items[n];
				items[n] = value;
			}
		}

		public void ScoreCurrent(int score)
		{
			items[CurrentIndex].AddScore(score);
			
			if (CurrentIndex < items.Count - 1)
			{
				CurrentIndex++;
			}
			else
			{
				OnWinnerChosen();
			}
		}

		private void OnWinnerChosen()
		{
			items = items.OrderByDescending(i => i.Score).ToList();
			WinnerSelected?.Invoke(items[0], items);
		}
	}

	public delegate void QuantificationWinnerSelectedEventHandler<T>(ScoredItem<T> winner, IEnumerable<ScoredItem<T>> standings) where T : class;
}
