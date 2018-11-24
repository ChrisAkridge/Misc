using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
	public sealed class BatchSelectionTournament<T> where T : class
	{
		public class Batch<TItem> where TItem : class
		{
			private int itemIndex = 0;
			
			public List<ScoredItem<TItem>> Items { get; private set; }

			public int BatchLevel { get; private set; }
			public int TotalScore => Items.Sum(i => i.Score);

			public Batch(int batchSize, int batchLevel, IEnumerable<TItem> items)
			{
				int expectedItemCount = (int)Math.Pow(batchSize, batchLevel + 1);
				this.Items = items.Select(i => new ScoredItem<TItem>(i)).ToList();
				if (this.Items.Count > expectedItemCount)
				{
					throw new ArgumentException($"Too many items for batch. Expected {expectedItemCount}, got {this.Items.Count}.");
				}
			}

			public Batch(int batchSize, int batchLevel, IEnumerable<ScoredItem<TItem>> items)
			{
				int expectedItemCount = (int)Math.Pow(batchSize, batchLevel + 1);
				Items = items.ToList();
				if (Items.Count > expectedItemCount)
				{
					throw new ArgumentException($"Too many items for batch. Expected {expectedItemCount}, got {Items.Count}.");
				}
			}

			public void AddScore(int amount)
			{
				foreach (ScoredItem<TItem> item in Items)
				{
					item.AddScore(amount);
				}
			}	
		}
		
		private readonly int BatchSize;
		private readonly List<ScoredItem<T>> items;

		private int roundNumber = -1;
		private List<Batch<T>> roundBatches;
		private int batchNumber = -1;

		private int TotalRounds => (int)Math.Ceiling(Math.Log((items.Count), BatchSize));

		public Batch<T> Current => roundBatches[batchNumber];
		public event NewBatchEventHandler<T> NewBatchEvent;
		public event WinnerSelectedEventHandler<T> WinnerSelectedEvent;

		public BatchSelectionTournament(IEnumerable<T> items, int batchSize)
		{
			if (items == null || !items.Any()) { throw new ArgumentException("There must be items."); }
			if (batchSize <= 0) { throw new ArgumentOutOfRangeException(nameof(batchSize), "A batch must have a positive number of items."); }

			this.items = items.Select(i => new ScoredItem<T>(i)).ToList();
			BatchSize = batchSize;

			NextRound();
		}

		private void NextRound()
		{
			roundNumber++;
			int newBatchSize = (int)Math.Pow(BatchSize, roundNumber + 1);

			if (newBatchSize >= items.Count)
			{
				WinnerSelected();
			}

			IEnumerable<ScoredItem<T>> itemsByScore;

			if (roundBatches == null)
			{
				itemsByScore = items;
			}
			else
			{
				IOrderedEnumerable<Batch<T>> batchesByScore = roundBatches.OrderByDescending(b => b.TotalScore);
				itemsByScore = batchesByScore.SelectMany(b => b.Items.OrderByDescending(i => i.Score));
			}

			var newBatches = new List<Batch<T>>();

			int remainingItems = items.Count;
			int itemsTaken = 0;
			while (remainingItems > 0)
			{
				IEnumerable<ScoredItem<T>> newBatchItems = itemsByScore.Skip(itemsTaken).Take(newBatchSize);
				remainingItems -= newBatchItems.Count();
				itemsTaken += newBatchItems.Count();
				var newBatch = new Batch<T>(BatchSize, roundNumber, newBatchItems);
				newBatches.Add(newBatch);
			}

			roundBatches = newBatches;
			batchNumber = 0;
		}

		private void NextBatch()
		{
			if (batchNumber == roundBatches.Count - 1)
			{
				NextRound();
			}
			else { batchNumber++; }

			OnNewBatch();
		}

		public void Start()
		{
			NextBatch();
		}

		public void SubmitScore(params Batch<T>[] batchOrder)
		{
			if (batchOrder.Length != BatchSize) { throw new ArgumentOutOfRangeException($"Invalid number of batches. Expected {BatchSize}, got {batchOrder.Length}"); }

			int score = batchOrder.Length;
			foreach (Batch<T> batch in batchOrder)
			{
				batch.AddScore(score);
				score--;
			}

			NextBatch();
		}

		private void WinnerSelected()
		{
			OnWinnerSelected();
		}

		private void OnNewBatch()
		{
			NewBatchEvent?.Invoke(Current);
		}

		private void OnWinnerSelected()
		{
			IEnumerable<ScoredItem<T>> allItems = roundBatches.SelectMany(b => b.Items);
			var order = (IOrderedEnumerable<T>)allItems.OrderByDescending(i => i.Score).Select(i => i.Item);

			WinnerSelectedEvent?.Invoke(order.First(), order);
		}
	}

	public sealed class ScoredItem<T> where T : class 
	{
		public T Item { get; }
		public int Score { get; private set; }

		public ScoredItem(T item)
		{
			Item = item;
			Score = 0;
		}

		public int AddScore(int amount) => Score += amount;

		public override string ToString() => $"{Score} points, {Item.ToString()}";
	}

	public delegate void NewBatchEventHandler<T>(BatchSelectionTournament<T>.Batch<T> newBatch) where T : class;

	public delegate void WinnerSelectedEventHandler<T>(T winner, IOrderedEnumerable<T> order);
}
