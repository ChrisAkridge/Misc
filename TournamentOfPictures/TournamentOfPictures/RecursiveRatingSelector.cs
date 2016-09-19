using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
	internal sealed class RecursiveRatingSelector<T> where T : class
	{
		// When the user rates the picture, add the rating to an item's score
		// As long as there are duplicate scores, keep iterating over each block of duplicates

		private Dictionary<int, List<T>> currentItemGroups;
		private Dictionary<int, List<T>> newItemGroups;
		private int currentKey;
		private int currentIndex;
		internal int CurrentGroupItemCount { get; private set; }
		private IEnumerator<int> keyEnumerator;

		public event EventHandler<List<T>> AllItemsRatedEvent;
		public event EventHandler<T> CurrentChangedEvent;
		public event EventHandler<int> NewScoreGroupEvent;

		public T Current
		{
			get
			{
				return currentItemGroups[currentKey][currentIndex];
			}
		}

		public RecursiveRatingSelector(IEnumerable<T> items)
		{
			if (items == null) { throw new ArgumentNullException(nameof(items), "The provided enumerable of items was null."); }
			if (!items.Any()) { throw new ArgumentException("The provided enumerable of items had no items.", nameof(items)); }

			int itemCount = items.Count();
			if (itemCount != items.Distinct().Count()) { throw new ArgumentException("The provided enumerable of items had duplicates.", nameof(items)); }

			currentItemGroups = new Dictionary<int, List<T>>();
			newItemGroups = new Dictionary<int, List<T>>();
			currentKey = 0;
			currentIndex = 0;

			currentItemGroups.Add(0, null);
			currentItemGroups[0] = items.ToList();
			CurrentGroupItemCount = currentItemGroups[currentKey].Count;

			keyEnumerator = currentItemGroups.Keys.GetEnumerator();
			keyEnumerator.MoveNext();
		}

		public void MoveNext()
		{
			if (currentIndex == CurrentGroupItemCount - 1)
			{
				// End of group, go to the next one
				if (!keyEnumerator.MoveNext())
				{
					// We're done with this round, check to see if all items have unique scores
					if (AllItemsHaveUniqueScores())
					{
						// We're done!
						OnAllItemsRated();
					}
					else
					{
						// Start the next round.
						StartNewRound();
						OnCurrentChanged();
						OnNewScoreGroup();
					}
				}
				else
				{
					// There is another group.
					currentKey = keyEnumerator.Current;
					CurrentGroupItemCount = currentItemGroups[currentKey].Count;
					currentIndex = 0;
					OnCurrentChanged();
					OnNewScoreGroup();
				}
			}
			else
			{
				currentIndex++;
				OnCurrentChanged();
			}
		}

		public void RateCurrent(int rating)
		{
			if (rating < 1 || rating > 5) { throw new ArgumentOutOfRangeException(nameof(rating), $"The provided rating {rating} was out of the range 1-5 inclusive."); }
			int currentRating = currentKey;
			int newRating = currentRating + rating;

			if (!newItemGroups.ContainsKey(newRating)) { newItemGroups.Add(newRating, new List<T>()); }
			newItemGroups[newRating].Add(Current);
			MoveNext();
		}

		private bool AllItemsHaveUniqueScores()
		{
			return newItemGroups.All(kvp => kvp.Value.Count == 1);
		}

		private void StartNewRound()
		{
			currentItemGroups = newItemGroups;
			newItemGroups = new Dictionary<int, List<T>>();
			keyEnumerator = currentItemGroups.Keys.OrderBy(s => s).GetEnumerator();
			keyEnumerator.MoveNext();

			currentKey = keyEnumerator.Current;
			CurrentGroupItemCount = currentItemGroups[currentKey].Count;
			currentIndex = 0;
		}

		private List<T> GenerateFinalList()
		{
			List<T> result = new List<T>();
			foreach (KeyValuePair<int, List<T>> itemGroup in currentItemGroups.OrderByDescending(kvp => kvp.Key))
			{
				result.AddRange(itemGroup.Value);
			}
			return result;
		}

		private void OnNewScoreGroup()
		{
			if (NewScoreGroupEvent != null)
			{
				NewScoreGroupEvent(this, currentKey);
			}
		}

		private void OnCurrentChanged()
		{
			if (CurrentChangedEvent != null)
			{
				CurrentChangedEvent(this, Current);
			}
		}

		private void OnAllItemsRated()
		{
			if (AllItemsRatedEvent != null)
			{
				AllItemsRatedEvent(this, GenerateFinalList());
			}
		}
	}
}
