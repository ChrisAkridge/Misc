using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
	internal sealed class HomeTeamAssigner
	{
		private readonly List<BasicTeamInfo> teams;
		private readonly List<GameMatchup> uniqueMatchups;
		private readonly Dictionary<BasicTeamInfo, int> teamHomeGameCounts;
		private readonly BasicTeamInfoComparer comparer = new();
		private readonly IRandom random;
		
		public HomeTeamAssigner(IEnumerable<BasicTeamInfo> teams,
			IEnumerable<GameMatchup> matchups,
			IRandomFactory randomFactory)
		{
			this.teams = [.. teams];
			random = randomFactory.Create(Helpers.SchedulingRandomSeed);

            var matchupsToPair = matchups.ToList();
			uniqueMatchups = new List<GameMatchup>(matchupsToPair.Count / 2);
			teamHomeGameCounts = this.teams.ToDictionary(t => t, t => 0, comparer);

			while (matchupsToPair.Count > 0)
			{
				var firstMatchup = matchupsToPair[0];
				var symmetricMatchup = FindSymmetricMatchup(matchupsToPair, firstMatchup);
				
				uniqueMatchups.Add(firstMatchup);
				matchupsToPair.Remove(firstMatchup);
				matchupsToPair.Remove(symmetricMatchup);
			}
		}

		public void AssignHomeTeams()
		{
			var teamAIsHomeTeamArray = new BitArray(320);
			
			// Assign the home/away randomly at first...
			for (var i = 0; i < uniqueMatchups.Count; i++)
			{
				var matchup = uniqueMatchups[i];
				var homeTeamIsTeamA = random.Next(2) == 0;
				matchup.HomeTeamIsTeamA = homeTeamIsTeamA;
				teamAIsHomeTeamArray.Set(i, homeTeamIsTeamA);
			}

			const int shuffleIterations = 256;
			var shuffles = 0;
			var bestTotalError = GetTotalError();
			var bestShuffleArray = new BitArray(teamAIsHomeTeamArray);

			Log.Information("Initial home/away total error is {InitialError}, running {Shuffles} shuffles", bestTotalError, shuffleIterations);
			
			while (shuffles < shuffleIterations && bestTotalError != 0)
			{
                var bestSwapIndex = BestSwapIndex();
				var matchup = uniqueMatchups[bestSwapIndex];
				matchup.HomeTeamIsTeamA = !matchup.HomeTeamIsTeamA;
				teamAIsHomeTeamArray.Set(bestSwapIndex, matchup.HomeTeamIsTeamA == true);
				
				var newTotalError = GetTotalError();
				if (newTotalError < bestTotalError)
				{
					Log.Information("Better home/away total error of {BetterError} found after {Shuffles} shuffles", newTotalError, shuffles);
					bestTotalError = newTotalError;
					bestShuffleArray = new BitArray(teamAIsHomeTeamArray);
				}

				shuffles += 1;
			}
			
			Log.Information("After {Shuffles} shuffles, lowest home/away total error was {BestError}", shuffleIterations, bestTotalError);
			
			for (int i = 0; i < uniqueMatchups.Count; i++)
			{
				var matchup = uniqueMatchups[i];
				var homeTeamIsTeamA = bestShuffleArray[i];
				AssignHomeTeam(matchup, homeTeamIsTeamA);
			}

			foreach (var team in teams)
			{
				var homeGameCount = teamHomeGameCounts[team];
				Log.Information("{Team} have {HomeGameCount} home games (error: {Error})", team.Name, homeGameCount, Math.Abs(8 - homeGameCount));
			}
		}

		private GameMatchup FindSymmetricMatchup(List<GameMatchup> matchups, GameMatchup matchup)
		{
			var otherTeamMatchups = matchups.Where(m => comparer.Equals(matchup.TeamB, m.TeamA));

			return otherTeamMatchups.First(m => (comparer.Equals(m.TeamB, matchup.TeamA))
				&& m.GameType == matchup.GameType
				&& m.HomeTeamIsTeamA == null);
		}
		
		private static void AssignHomeTeam(GameMatchup matchup, bool homeTeamIsTeamA)
		{
			Log.Information("Matchup of {TeamA} vs. {TeamB} will have a home team of {HomeTeam} ({GameType})",
				matchup.TeamA.Name, matchup.TeamB.Name,
				homeTeamIsTeamA
					? matchup.TeamA.Name
					: matchup.TeamB.Name,
				matchup.GameType);

			matchup.HomeTeamIsTeamA = homeTeamIsTeamA;
		}

		private int GetTotalError()
		{
			// Clear the error buffer by setting all values to 0.
			foreach (var team in teams)
			{
				teamHomeGameCounts[team] = 0;
			}

			foreach (var uniqueMatchup in uniqueMatchups)
			{
				var homeTeam = uniqueMatchup.HomeTeamIsTeamA == true
					? uniqueMatchup.TeamA
					: uniqueMatchup.TeamB;
				teamHomeGameCounts[homeTeam] += 1;
			}

			return teams.Sum(team => Math.Abs(8 - teamHomeGameCounts[team]));
		}

		private int BestSwapIndex()
		{
			// We'll use Gradient Descent here, which lets us figure out which swap of the 320 would
			// reduce the error the most, or at least increase it the least. Remember, every swap
			// is only ever a +/-1 to a team's home count, and a +/-2 to the overall error.

			var bestErrorDelta = int.MaxValue;
			var bestSwapIndex = -1;
			
			for (var i = 0; i < uniqueMatchups.Count; i++)
			{
				var matchup = uniqueMatchups[i];
				var teamAHomeCount = teamHomeGameCounts[matchup.TeamA];
				var teamBHomeCount = teamHomeGameCounts[matchup.TeamB];
				var teamACurrentError = Math.Abs(8 - teamAHomeCount);
				var teamBCurrentError = Math.Abs(8 - teamBHomeCount);

				var teamAHomeGamesAfterSwap = matchup.HomeTeamIsTeamA == true
					? teamAHomeCount - 1
					: teamAHomeCount + 1;
				var teamBHomeGamesAfterSwap = matchup.HomeTeamIsTeamA == true
					? teamBHomeCount + 1
					: teamBHomeCount - 1;
				
				var teamANewError = Math.Abs(8 - teamAHomeGamesAfterSwap);
				var teamBNewError = Math.Abs(8 - teamBHomeGamesAfterSwap);
				var errorDelta = (teamANewError + teamBNewError) - teamACurrentError - teamBCurrentError;
				
				if (errorDelta < bestErrorDelta)
				{
					bestErrorDelta = errorDelta;
					bestSwapIndex = i;
				}
			}

			return bestSwapIndex;
		}
	}
}
