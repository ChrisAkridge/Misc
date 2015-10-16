using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TournamentOfPictures
{
    public sealed class BracketedTournament<T> where T : class
    {
        public List<T> teams;
        public T OverflowItem = null;
        public T TournamentWinner = null;
        public int TeamCount
        {
            get
            {
                return teams.Count;
            }
        }
        public OddTeamCountBehavior Behavior = OddTeamCountBehavior.RemoveRandomly;
        public BracketedTournamentRound<T> currentRound;
        public int currentRoundNumber { get; private set; }
        public event WinnerChosenEventHandler<T> WinnerChosenEvent;

		private Dictionary<T, int> standings = new Dictionary<T,int>();

        public BracketedTournament(List<T> teams)
        {
            this.teams = teams;
			this.teams.ForEach(t => standings.Add(t, 0));
			currentRoundNumber = 0;
			ShuffleTeams();
        }

        public void AddToTop(T item)
        {
			teams.Insert(0, item);
        }

        public void AddToBottom(T item)
        {
			teams.Add(item);
        }

        public T RemoveFromTop()
        {
            if (teams.Any())
            {
                T result = teams[0];
				teams.RemoveAt(0);
                return result;
            }
            else
            {
                throw new Exception("Teams list is empty; cannot return a value");
            }
        }

        public T RemoveFromBottom()
        {
            if (teams.Any())
            {
                int index = teams.Count - 1;
                T result = teams[index];
				teams.RemoveAt(index);
                return result;
            }
            else
            {
                throw new Exception("Teams list is empty, cannot return result");
            }
        }

		public T RemoveRandomly()
		{
			if (teams.Any())
			{
				Random random = new Random();
				int index = random.Next(0, teams.Count);
				T result = teams[index];
				teams.RemoveAt(index);
				return result;
			}
			else
			{
				throw new Exception("Teams list is empty, cannot return result");
			}
		}

		public T RemoveAndInsertRandomly(ref T item)
		{
			if (teams.Any())
			{
				Random random = new Random();
				int index = random.Next(0, teams.Count);
				T result = teams[index];
				teams[index] = item;
				item = result;
				return result;
			}
			else
			{
				throw new Exception("Teams list is empty, cannot return result");
			}
		}

        public void ShuffleTeams()
        {
            Random random = new Random(DateTime.Now.Minute * DateTime.Now.Second * DateTime.Now.Millisecond);
            int n = teams.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = teams[k];
				teams[k] = teams[n];
				teams[n] = value;
            }
        }

        public void StartRound()
        {
            var newRound = new BracketedTournamentRound<T>(this);
			currentRound = newRound;
			currentRoundNumber++;
        }

        public void GetRoundWinners(List<T> winners)
        {
			teams = winners;
			winners.ForEach(w => standings[w]++); // increase the wins by one
			currentRound = null;

            if (winners.Count == 1 && OverflowItem == null)
            {
				OnWinnerChosen(winners[0], GetFinalStandings());
            }
        }

        public void OnWinnerChosen(T winner, string standings)
        {
            if (WinnerChosenEvent != null)
            {
				WinnerChosenEvent(winner, standings);
            }
        }

		public string GetFinalStandings()
		{
			StringBuilder result = new StringBuilder();
			int i = 1;
			foreach (var value in standings.OrderByDescending(key => key.Value))
			{
				result.Append(string.Format("{0}. {1} ({2} win{3}){4}", i, 
																		value.Key, 
																		value.Value, 
																		(value.Value != 1) ? "s" : "", 
																		Environment.NewLine));
				i++;
			}
			return result.ToString();
		}
    }

    public sealed class BracketedTournamentRound<T> where T : class
    {
        private List<BracketedTournamentMatch<T>> matches;
        private List<T> winners;
        private BracketedTournament<T> owner;
        public int matchesRemaining;

        public BracketedTournamentRound(BracketedTournament<T> owner)
        {
			matches = new List<BracketedTournamentMatch<T>>();
			winners = new List<T>();
            this.owner = owner;

            if (owner.TeamCount % 2 != 0)
            {
                // odd number of teams
                if (owner.OverflowItem == null)
                {
                    // overflow is empty
					if (owner.Behavior == OddTeamCountBehavior.RemoveFromTop)
					{
						owner.OverflowItem = owner.RemoveFromTop();
					}
					else if (owner.Behavior == OddTeamCountBehavior.RemoveFromBottom)
					{
						owner.OverflowItem = owner.RemoveFromBottom();
					}
					else if (owner.Behavior == OddTeamCountBehavior.RemoveRandomly)
					{
						owner.OverflowItem = owner.RemoveRandomly();
					}
                }
                else
                {
                    // overflow isn't empty; restore team
                    if (owner.Behavior == OddTeamCountBehavior.RemoveFromTop)
                    {
                        owner.teams.Insert(0, owner.OverflowItem);
                    }
                    else
                    {
                        owner.teams.Add(owner.OverflowItem);
                    }
                    owner.OverflowItem = null;
                }
            }
			else if (owner.OverflowItem != null && owner.Behavior == OddTeamCountBehavior.RemoveRandomly && owner.TeamCount % 2 == 0)
			{
				// Swap out the overflow item in rounds so it stays relatively fresh
				// This avoids giving a random item a mega-bye and having it face off against the best of the best
				owner.RemoveAndInsertRandomly(ref owner.OverflowItem);
			}
            else if (owner.TeamCount == 1)
            {
                if (owner.OverflowItem == null)
                {
                    // there's an overflow item left
                    owner.teams.Add(owner.OverflowItem);
                    owner.OverflowItem = null;
                }
                else
                {
                    owner.TournamentWinner = owner.teams[0];
					return;
                }
            }
            else if (owner.TeamCount <= 0)
            {
                throw new Exception("Cannot run match with no teams");
            }

            // By this point, we have an even number of teams
            // We'll use the ordering of the list to build the matches

            for (int i = 0; i < owner.TeamCount; i += 2)
            {
                BracketedTournamentMatch<T> match = new BracketedTournamentMatch<T>(owner.teams[i], owner.teams[i + 1], this);
				matches.Add(match);
				matchesRemaining++;
            }
        }

        public BracketedTournamentMatch<T> GetNextMatch()
        {
            if (matches.Any())
            {
                return matches[0];
            }
            else return null;
        }

        public void SelectNextMatchWinner(int teamNumber)
        {
            if (matches.Any() && matchesRemaining > 0)
            {
                var nextMatch = GetNextMatch();
				matches.RemoveAt(0);
				matchesRemaining--;

                if (teamNumber == 1)
                {
					winners.Add(nextMatch.team1);
                }
                else if (teamNumber == 2)
                {
					winners.Add(nextMatch.team2);
                }
                else
                {
                    throw new Exception(string.Format("Invalid team number {0}", teamNumber));
                }

                if (matchesRemaining == 0)
                {
					owner.GetRoundWinners(winners);
                }
            }
        }
    }

    public sealed class BracketedTournamentMatch<T> where T : class
    {
        public T team1;
        public T team2;
        public int Winner { get; private set; }

        private BracketedTournamentRound<T> ownerRound;

        public BracketedTournamentMatch(T a, T b, BracketedTournamentRound<T> owner)
        {
			team1 = a;
			team2 = b;
			ownerRound = owner;
        }

        public void SelectWinner(int teamNumber)
        {
            if (teamNumber < 1 || teamNumber > 2)
            {
                throw new IndexOutOfRangeException(string.Format("Cannot select team #{0} from two teams", teamNumber));
            }
			Winner = teamNumber;
        }
    }

    public enum OddTeamCountBehavior
    {
        RemoveFromTop,
        RemoveFromBottom,
		RemoveRandomly
    }

    public delegate void WinnerChosenEventHandler<T>(T winner, string standings);
}
