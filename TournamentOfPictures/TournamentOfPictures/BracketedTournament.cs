using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TournamentOfPictures
{
	public sealed class BracketedTournament<T> where T : class
	{
		private Dictionary<T, int> standings = new Dictionary<T, int>();
		private List<BracketedTournamentRound<T>> previousRounds = new List<BracketedTournamentRound<T>>();
		internal T overflowItem = null;

		public event WinnerChosenEventHandler<T> WinnerChosenEvent;

		public OddTeamCountBehavior Behavior { get; private set; } = OddTeamCountBehavior.RemoveRandomly;
		public BracketedTournamentRound<T> CurrentRound { get; private set; }
		public int CurrentRoundNumber { get; internal set; }
		public T OverflowItem
		{
			get { return overflowItem; }
			set { overflowItem = value; }
		}
		public int TeamCount => Teams.Count;
		public List<T> Teams { get; set; }
		public T TournamentWinner { get; internal set; } = null;

		public bool CanUndo
		{
			get
			{
				if (CurrentRound != null && CurrentRoundNumber == 1) { return CurrentRound.CurrentMatchIndex > 0; }
				else { return true; }
			}
		}

		public BracketedTournament(List<T> teams)
		{
			Teams = teams;
			Teams.ForEach(t => standings.Add(t, 0));
			CurrentRoundNumber = 0;
			ShuffleTeams();
		}

		internal BracketedTournament()
		{
			
		}

		public void AddToBottom(T item)
		{
			Teams.Add(item);
		}

		public void AddToTop(T item)
		{
			Teams.Insert(0, item);
		}

		public IEnumerable<ScoredItem<T>> GetFinalStandings()
		{
			return standings.Select(kvp =>
			{
				ScoredItem<T> item = new ScoredItem<T>(kvp.Key);
				item.AddScore(kvp.Value);
				return item;
			});
		}

		public void GetRoundWinners(List<T> winners)
		{
			Teams = winners;
			winners.ForEach(w => standings[w]++); // increase the wins by one
			previousRounds.Add(CurrentRound);
			CurrentRound = null;

			if (winners.Count == 1 && OverflowItem == null)
			{
				OnWinnerChosen(winners[0], GetFinalStandings());
			}
		}

		public void OnWinnerChosen(T winner, IEnumerable<ScoredItem<T>> standings)
		{
			WinnerChosenEvent?.Invoke(winner, standings);
		}

		public T RemoveAndInsertRandomly(ref T item)
		{
			if (Teams.Any())
			{
				Random random = new Random();
				int index = random.Next(0, Teams.Count);
				T result = Teams[index];
				Teams[index] = item;
				item = result;
				return result;
			}
			else
			{
				throw new Exception("Teams list is empty, cannot return result");
			}
		}

		public T RemoveFromBottom()
		{
			if (Teams.Any())
			{
				int index = Teams.Count - 1;
				T result = Teams[index];
				Teams.RemoveAt(index);
				return result;
			}
			else
			{
				throw new Exception("Teams list is empty, cannot return result");
			}
		}

		public T RemoveFromTop()
		{
			if (Teams.Any())
			{
				T result = Teams[0];
				Teams.RemoveAt(0);
				return result;
			}
			else
			{
				throw new Exception("Teams list is empty; cannot return a value");
			}
		}
		public T RemoveRandomly()
		{
			if (Teams.Any())
			{
				Random random = new Random();
				int index = random.Next(0, Teams.Count);
				T result = Teams[index];
				Teams.RemoveAt(index);
				return result;
			}
			else
			{
				throw new Exception("Teams list is empty, cannot return result");
			}
		}
		public void ShuffleTeams()
		{
			Random random = new Random();
			int n = Teams.Count;

			while (n > 1)
			{
				n--;
				int k = random.Next(n + 1);
				T value = Teams[k];
				Teams[k] = Teams[n];
				Teams[n] = value;
			}
		}

		public void StartRound()
		{
			var newRound = new BracketedTournamentRound<T>(this);
			CurrentRound = newRound;
			CurrentRoundNumber++;
		}

		public void Undo()
		{
			CurrentRound.Undo();
		}

		internal void GoBackToPreviousRound()
		{
			var previousRound = previousRounds.Last();
			previousRounds.Remove(previousRound);
			CurrentRoundNumber--;
			CurrentRound = previousRound;
			CurrentRound.Undo();
			Teams = CurrentRound.GetTeamsForUndo();
		}

		public object GetSerializableObjects()
		{
			List<object> previousRoundsObjects = new List<object>();
			List<object> standingsObjects = new List<object>();

			previousRounds.ForEach(r => previousRoundsObjects.Add(r.GetSerializableObjects()));
			
			foreach (var standing in standings)
			{
				standingsObjects.Add(new
				{
					team = standing.Key,
					wins = standing.Value
				});
			}

			return new
			{
				currentRoundNumber = CurrentRoundNumber,
				standings = standingsObjects,
				previousRounds = previousRoundsObjects,
				currentRound = CurrentRound.GetSerializableObjects()
			};
		}

		internal void SerializationSetStandings(IDictionary<T, int> standings)
		{
			this.standings = (Dictionary<T, int>)standings;
		}

		internal void SerializationAddPreviousRound(BracketedTournamentRound<T> round)
		{
			previousRounds.Add(round);
		}

		internal void SerializationSetCurrentRound(BracketedTournamentRound<T> round)
		{
			CurrentRound = round;
			Teams = CurrentRound.GetTeamsForUndo();
		}
	}

	public sealed class BracketedTournamentRound<T> where T : class
	{
		private BracketedTournament<T> owner;
		private List<BracketedTournamentMatch<T>> matches = new List<BracketedTournamentMatch<T>>();
		private List<T> winners = new List<T>();
		internal int CurrentMatchIndex { get; private set; } = 0;  // -1 for all matches selected
		public int TotalMatches => matches.Count;

		public BracketedTournamentMatch<T> CurrentMatch
		{
			get
			{
				if (CurrentMatchIndex != -1 && matches.Any()) { return matches[CurrentMatchIndex]; }
				return null;
			}
		}
		
		public int MatchesRemaining { get; private set; }

		public BracketedTournamentRound(BracketedTournament<T> owner)
		{
			this.owner = owner;

			if (owner.TeamCount % 2 != 0)
			{
				if (owner.OverflowItem == null)
				{
					SelectOverflowItem();
				}
				else
				{
					if (owner.Behavior == OddTeamCountBehavior.RemoveFromTop) { owner.Teams.Insert(0, owner.OverflowItem); }
					else { owner.Teams.Add(owner.OverflowItem); }
					owner.OverflowItem = null;
				}
			}
			else if (owner.OverflowItem != null && owner.Behavior == OddTeamCountBehavior.RemoveRandomly && owner.TeamCount % 2 == 0)
			{
				// Swap out the overflow item in rounds so it stays relatively fresh
				// This avoids giving a random item a mega-bye and having it face off against the best of the best
				owner.RemoveAndInsertRandomly(ref owner.overflowItem);
			}
			else if (owner.TeamCount == 1)
			{
				if (owner.OverflowItem != null)
				{
					owner.Teams.Add(owner.OverflowItem);
					owner.OverflowItem = null;
				}
				else
				{
					owner.TournamentWinner = owner.Teams[0];
					return;
				}
			}
			
			for (int i = 0; i < owner.TeamCount; i += 2)
			{
				matches.Add(new BracketedTournamentMatch<T>(owner.Teams[i], owner.Teams[i + 1], this));
				MatchesRemaining++;
			}
		}

		internal BracketedTournamentRound(BracketedTournament<T> owner, IEnumerable<BracketedTournamentMatch<T>> matches)
		{
			this.owner = owner;
			this.matches = matches.ToList();

			int indexOfFirstUnselectedMatch = this.matches.FindIndex(m => m.WinnerIndex == 0);
			if (indexOfFirstUnselectedMatch >= 0)
			{
				CurrentMatchIndex = indexOfFirstUnselectedMatch;
			}
			else
			{
				CurrentMatchIndex = -1;
			}
		}

		private void SelectOverflowItem()
		{
			switch (owner.Behavior)
			{
				case OddTeamCountBehavior.RemoveFromTop:
					owner.OverflowItem = owner.RemoveFromTop();
					break;
				case OddTeamCountBehavior.RemoveFromBottom:
					owner.OverflowItem = owner.RemoveFromBottom();
					break;
				case OddTeamCountBehavior.RemoveRandomly:
					owner.OverflowItem = owner.RemoveRandomly();
					break;
				default:
					break;
			}
		}

		public void SelectMatchWinner(int teamNumber)
		{
			if (teamNumber != 1 && teamNumber != 2) { throw new ArgumentOutOfRangeException(); }
			CurrentMatch.SelectWinner(teamNumber);
			winners.Add(CurrentMatch.Winner);
			MatchesRemaining--;

			if (CurrentMatchIndex < matches.Count - 1)
			{
				CurrentMatchIndex++;
			}
			else
			{
				owner.GetRoundWinners(winners);
				CurrentMatchIndex = -1;
			}
		}

		public void Undo()
		{
			// Base case: this is the first match and we have to go back
			if (CurrentMatchIndex == 0)
			{
				owner.GoBackToPreviousRound();
			}
			// We also have to set up the last round
			else if (CurrentMatchIndex == -1)
			{
				CurrentMatchIndex = matches.Count - 1;
				CurrentMatch.ClearWinner();
			}
			else
			{
				CurrentMatchIndex--;
				CurrentMatch.ClearWinner();
			}
		}

		internal List<T> GetTeamsForUndo()
		{
			List<T> result = new List<T>();
			foreach (var match in matches)
			{
				result.Add(match.Team1);
				result.Add(match.Team2);
			}
			return result;
		}

		public object GetSerializableObjects()
		{
			List<object> matchObjects = new List<object>();
			matches.ForEach(m => matchObjects.Add(m.GetSerializaleObjects()));

			return new
			{
				matches = matchObjects
			};
		}
	}

	public sealed class BracketedTournamentMatch<T> where T : class
	{
		public T Team1 { get; private set; }
		public T Team2 { get; private set; }
		public int WinnerIndex { get; private set; } = 0;
		public T Winner
		{
			get
			{
				if (WinnerIndex == 0) { return null; }
				else if (WinnerIndex == 1) { return Team1; }
				else { return Team2; }
			}
		}

		public BracketedTournamentMatch(T a, T b, BracketedTournamentRound<T> owner)
		{
			Team1 = a;
			Team2 = b;
		}

		internal BracketedTournamentMatch(T a, T b, int winnerIndex)
		{
			Team1 = a;
			Team2 = b;
			WinnerIndex = winnerIndex;
		}

		public void SelectWinner(int teamNumber)
		{
			if (teamNumber < 1 || teamNumber > 2)
			{
				throw new IndexOutOfRangeException(string.Format("Cannot select team #{0} from two teams", teamNumber));
			}
			WinnerIndex = teamNumber;
		}

		internal void ClearWinner()
		{
			WinnerIndex = 0;
		}

		public object GetSerializaleObjects()
		{
			return new
			{
				team1 = Team1,
				team2 = Team2,
				winnerIndex = WinnerIndex
			};
		}
	}

	public delegate void WinnerChosenEventHandler<T>(T winner, IEnumerable<ScoredItem<T>> standings) where T : class;

	public enum OddTeamCountBehavior
	{
		RemoveFromTop,
		RemoveFromBottom,
		RemoveRandomly
	}
}