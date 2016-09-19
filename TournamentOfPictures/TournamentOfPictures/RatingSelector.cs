using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
	internal sealed class RatingSelector<T>
	{
		private Dictionary<T, int> items;
		private int currentKeyIndex;
		private int itemCount;

		public event EventHandler<List<T>> AllItemsRatedEvent;
		public event EventHandler<T> CurrentChangedEvent;

		public T Current
		{
			get
			{
				return items.Keys.ElementAt(currentKeyIndex);
			}
		}

		public RatingSelector(IEnumerable<T> items)
		{
			if (items == null) { throw new ArgumentNullException(nameof(items), "The provided enumerable of items was null."); }
			if (!items.Any()) { throw new ArgumentException("The provided enumerable of items had no items.", nameof(items)); }

			itemCount = items.Count();
			if (itemCount != items.Distinct().Count()) { throw new ArgumentException("The provided enumerable of items had duplicates.", nameof(items)); }

			this.items = new Dictionary<T, int>(itemCount);
			foreach (T item in items)
			{
				// Items with a rating of 0 are considered not rated
				this.items.Add(item, 0);
			}
			currentKeyIndex = 0;
		}

		public void MoveNext()
		{
			if (currentKeyIndex == itemCount - 1)
			{
				OnAllItemsRated();
			}
			else
			{
				currentKeyIndex++;
				OnCurrentChanged();
			}
		}

		public void RateCurrent(int rating)
		{
			if (rating < 1 || rating > 5) { throw new ArgumentOutOfRangeException(nameof(rating), $"The provided rating {rating} was out of the range 1-5 inclusive."); }
			items[items.Keys.ElementAt(currentKeyIndex)] = rating;
			MoveNext();
		}

		private void OnAllItemsRated()
		{
			if (AllItemsRatedEvent != null)
			{
				List<T> sortedItems = items.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
				AllItemsRatedEvent(this, sortedItems);
			}
		}

		private void OnCurrentChanged()
		{
			if (CurrentChangedEvent != null)
			{
				CurrentChangedEvent(this, Current);
			}
		}
	}
}
