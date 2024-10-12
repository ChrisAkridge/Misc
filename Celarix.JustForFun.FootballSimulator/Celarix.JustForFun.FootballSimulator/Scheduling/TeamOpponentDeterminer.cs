﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Serilog;
using static Celarix.JustForFun.FootballSimulator.Helpers;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
	internal sealed class TeamOpponentDeterminer
	{
		private readonly IReadOnlyList<BasicTeamInfo> teams;
		private readonly BasicTeamInfoComparer comparer = new();
		
		public TeamOpponentDeterminer(IReadOnlyList<BasicTeamInfo> teams) => this.teams = teams;
		
		public List<GameMatchup> GetTeamOpponentsForSeason(int cycleYear,
			Dictionary<BasicTeamInfo, int>? previousSeasonDivisionRankings)
		{
			var matchups = new List<GameMatchup>();
			previousSeasonDivisionRankings ??= GetDefaultPreviousSeasonDivisionRankings();

			LogPreviousSeasonRankings(previousSeasonDivisionRankings);

			foreach (var team in teams)
			{
				AddIntradivisionGamesForTeam(team, matchups);
				AddIntraconferenceMatchupsForTeam(cycleYear, team, matchups);
				AddInterconferenceMatchupsForTeam(cycleYear, team, matchups);
				AddRemainingIntraconferenceMatchupsForTeam(cycleYear, previousSeasonDivisionRankings, team, matchups);
			}

			ValidateOrThrow(matchups);
			
			return matchups;
		}
		
		// Matchup Generation
		
		private static GameMatchup CreateMatchup(BasicTeamInfo teamA, BasicTeamInfo teamB, ScheduledGameType gameType) =>
			new GameMatchup
			{
				TeamA = teamA,
				TeamB = teamB,
				GameType = gameType
			};

		private void AddIntradivisionGamesForTeam(BasicTeamInfo team, List<GameMatchup> matchups)
		{
			// Intradivision: 2 games each against 3 division opponents (6 games total)
			foreach (var intradivisionOpponent in GetTeamsInDivision(teams, team.Conference, team.Division)
				         .Where(t => !comparer.Equals(t, team)))
			{
				AddMatchup(matchups, CreateMatchup(team, intradivisionOpponent, ScheduledGameType.IntradivisionalFirstSet));
				AddMatchup(matchups, CreateMatchup(team, intradivisionOpponent, ScheduledGameType.IntradivisionalSecondSet));
			}
		}

		private void AddIntraconferenceMatchupsForTeam(int cycleYear, BasicTeamInfo team, List<GameMatchup> matchups)
		{
			// Intraconference: 1 game against each of the 4 teams in another division in the same conference (4 games total)
			var intraconferenceOpponents = GetIntraconferenceOpponentsForTeam(team, cycleYear).GroupBy(o => o.Name).ToArray();

			if (intraconferenceOpponents.Length == 3)
			{
				var doubleOpponent = intraconferenceOpponents.First(g => g.Count() == 2).First();
				AddMatchup(matchups, CreateMatchup(team, doubleOpponent, ScheduledGameType.IntraconferenceFirstSet));
				AddMatchup(matchups, CreateMatchup(team, doubleOpponent, ScheduledGameType.IntraconferenceSecondSet));

				var firstSetOpponents = intraconferenceOpponents.Where(g => g.Count() == 1)
					.Select(g => g.First())
					.Select((o, i) => CreateMatchup(team, o, ScheduledGameType.IntraconferenceFirstSet));

				AddMatchupRange(matchups, firstSetOpponents);
			}
			else
			{
				var firstSetOpponents = intraconferenceOpponents.SelectMany(g => g)
					.Select((o, i) => CreateMatchup(team, o, ScheduledGameType.IntraconferenceFirstSet));

				AddMatchupRange(matchups, firstSetOpponents);
			}
		}

		private void AddInterconferenceMatchupsForTeam(int cycleYear, BasicTeamInfo team, List<GameMatchup> matchups)
		{
			// Interconference: 1 game against each of the 4 teams in a division in the other conference (4 games total)
			var interconferenceMatchups = GetTeamsInDivision(teams,
					team.Conference.OtherConference(),
					DivisionMatchupCycles.GetInterconferenceOpponentDivision(cycleYear, team.Conference, team.Division))
				.Select((o, i) => CreateMatchup(team, o, ScheduledGameType.Interconference));

			AddMatchupRange(matchups, interconferenceMatchups);
		}

		private void AddRemainingIntraconferenceMatchupsForTeam(int cycleYear, Dictionary<BasicTeamInfo, int> previousSeasonDivisionRankings,
			BasicTeamInfo team, List<GameMatchup> matchups)
		{
			// Remaining intraconference: 1 game against 2 teams in 2 other divisions that finished in the same position as the team did (2 games total)
			var remainingIntraconferenceOpponents = GetRemainingIntraconferenceOpponentTeams(team, cycleYear, previousSeasonDivisionRankings).ToArray();

			if (comparer.Equals(remainingIntraconferenceOpponents[0], remainingIntraconferenceOpponents[1]))
			{
				// We're in the year when the division plays itself for the remaining intraconference games.
				AddMatchup(matchups, CreateMatchup(team, remainingIntraconferenceOpponents[0], ScheduledGameType.RemainingIntraconferenceFirstSet));
				AddMatchup(matchups, CreateMatchup(team, remainingIntraconferenceOpponents[0], ScheduledGameType.RemainingIntraconferenceSecondSet));
			}
			else
			{
				var remainingIntraconferenceFirstSetMatchups = remainingIntraconferenceOpponents
					.Select((o, i) => CreateMatchup(team, o, ScheduledGameType.RemainingIntraconferenceFirstSet));
				AddMatchupRange(matchups, remainingIntraconferenceFirstSetMatchups);
			}
		}

		private static void LogPreviousSeasonRankings(Dictionary<BasicTeamInfo, int> previousSeasonDivisionRankings)
		{
			var rankingsForLogging = string.Join(Environment.NewLine, previousSeasonDivisionRankings
				.GroupBy(kvp => new
				{
					kvp.Key.Conference,
					kvp.Key.Division
				})
				.Select(g =>
				{
					var builder = new StringBuilder();
					builder.Append($"{g.Key.Conference} {g.Key.Division}: ");
					var teamsInOrder = g.OrderBy(kvp => kvp.Value);

					foreach (var teamKVP in teamsInOrder) { builder.Append($"#{teamKVP.Value} {teamKVP.Key.Name}, "); }

					builder.Remove(builder.Length - 2, 2);

					return builder.ToString();
				}));
			Log.Information("Previous season division rankings:");
			Log.Information(rankingsForLogging);
		}

		private static void AddMatchup(ICollection<GameMatchup> matchups, GameMatchup matchup)
		{
			Log.Information("Adding matchup #{GameNumber}: {TeamA} vs. {TeamB} ({GameType})",
				matchups.Count, matchup.TeamA.Name, matchup.TeamB.Name, matchup.GameType);

			matchups.Add(matchup);
		}
		
		private static void AddMatchupRange(ICollection<GameMatchup> matchups, IEnumerable<GameMatchup> newMatchups)
		{
			foreach (var matchup in newMatchups)
			{
				AddMatchup(matchups, matchup);
			}
		}

		private Dictionary<BasicTeamInfo, int> GetDefaultPreviousSeasonDivisionRankings()
		{
			Log.Information("No previous season division rankings available, generating default...");

			var random = new Random(-1528635010);
			var rankings = new Dictionary<BasicTeamInfo, int>();

			var conferences = new[]
			{
				Conference.AFC, Conference.NFC
			};

			var divisions = new[]
			{
				Division.East, Division.North, Division.South, Division.West, Division.Extra
			};

			foreach (var divisionTeams in conferences.SelectMany(c => divisions.Select(d => GetTeamsInDivision(teams, c, d).ToList())))
			{
				divisionTeams.Shuffle(random);

				for (int i = 0; i < 4; i++) { rankings[divisionTeams[i]] = i + 1; }
			}

			return rankings;
		}

		private IEnumerable<BasicTeamInfo> GetIntraconferenceOpponentsForTeam(BasicTeamInfo team, int cycleYear)
		{
			var opponentDivision = DivisionMatchupCycles.GetIntraconferenceOpponentDivision(cycleYear, team.Division);
			if (opponentDivision == team.Division)
			{
				// Intraconference games fill 4 slots, but we can't play ourselves.
				// So we need to play 1 game against 2 opponents and 2 games against the third.
				// We'd like to choose semi-randomly. Let's start by seeding an RNG with the cycle year.
				var random = new Random(cycleYear);

				// There are 16 symmetric slots to fill for these teams in this division:
				// Colts   . . . . 
				// Texans  . . . .
				// Jaguars . . . .
				// Titans  . . . .
				// Since there are 16 symmetric slots, we only need to fill 8 of them.
				// How can we choose randomly such that each team plays once against 2 opponents and
				// twice against the third? Well, there is still a symmetry here. Let's say the Colts
				// play twice against the Jaguars. Symmetrically, the Jaguars play twice against the Colts.
				// That leaves us with two teams, the Titans and the Texans, who play against each other twice.
				// Let's start by shuffling the teams using the random number generator.
				var divisionTeams = GetTeamsInDivision(teams, team.Conference, team.Division).ToList();
				divisionTeams.Shuffle(random);

				// Now we have the four teams in a pseudo-random order. We can pair them up.
				// The team at index 0 will play twice against the team at index 1 and the
				// team at index 2 will play twice against the team at index 3.
				var teamIndex = divisionTeams.IndexOf(team);
				var twiceOpponentIndex = teamIndex switch
				{
					0 => 1,
					1 => 0,
					2 => 3,
					3 => 2,
					_ => throw new InvalidOperationException("Unreachable: Somehow shuffled a 4-item list and got more than 4 items.")
				};

				yield return divisionTeams[twiceOpponentIndex];
				yield return divisionTeams[twiceOpponentIndex];

				divisionTeams.RemoveAt(twiceOpponentIndex);
				divisionTeams.Remove(team);

				foreach (var remainingOpponent in divisionTeams)
				{
					yield return remainingOpponent;
				}

				// Ugh. Quite inelegant.
			}
			else
			{
				foreach (var opponentTeam in GetTeamsInDivision(teams, team.Conference, opponentDivision))
				{
					yield return opponentTeam;
				}
			}
		}

		private IEnumerable<BasicTeamInfo> GetRemainingIntraconferenceOpponentTeams(BasicTeamInfo team,
			int cycleYear,
			IReadOnlyDictionary<BasicTeamInfo, int> previousSeasonDivisionRankings)
		{
			var opponentDivisions = DivisionMatchupCycles.GetRemainingIntraconferenceOpponentDivisions(cycleYear, team.Division);

			if (team.Division != opponentDivisions.Item1)
			{
				var teamsInDivisions = GetTeamsInDivision(teams, team.Conference, opponentDivisions.Item1)
					.Concat(GetTeamsInDivision(teams, team.Conference, opponentDivisions.Item2));
				var teamRanking = previousSeasonDivisionRankings[team];

				return teamsInDivisions.Where(t => previousSeasonDivisionRankings[t] == teamRanking);
			}

			// A team faces its own division again every five years. #4 plays #1 and #3 plays #2 twice.
			var divisionTeams = GetTeamsInDivision(teams, team.Conference, team.Division).ToArray();

			return Enumerable.Repeat(previousSeasonDivisionRankings[team] switch
			{
				1 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 4),
				2 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 3),
				3 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 2),
				4 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 1),
				_ => throw new InvalidOperationException("Invalid division ranking.")
			}, 2);
		}
		
		// Matchup Validation
		private void ValidateOrThrow(ICollection<GameMatchup> matchups)
		{
			var builder = new StringBuilder();
			builder.AppendLine(Has640Matchups(matchups));
			builder.AppendLine(EachTeamAppears32Times(matchups));
			builder.AppendLine(EachTeamHasProperNumberOfGameTypes(matchups));
			
			var errors = builder.ToString().Trim();

			if (errors != string.Empty)
			{
				throw new InvalidOperationException(errors);
			}
		}
		
		private string Has640Matchups(ICollection<GameMatchup> matchups) =>
			matchups.Count == 640
				? string.Empty
				: $"Expected 640 matchups, but found {matchups.Count}";
		
		private string EachTeamAppears32Times(ICollection<GameMatchup> matchups) =>
			teams.All(t => matchups.Count(m => comparer.Equals(m.TeamA, t) || comparer.Equals(m.TeamB, t)) == 32)
				? string.Empty
				: "Each team should appear 32 times in the matchups";

		private string EachTeamHasProperNumberOfGameTypes(ICollection<GameMatchup> matchups)
		{
			var matchupsByTeam = matchups.GroupBy(m => m.TeamA);
			var allTeamsHaveCorrectNumberOfGamesByType = true;

			foreach (var team in matchupsByTeam)
			{
				var intradivisionalCount = team.Count(m => m.GameType is ScheduledGameType.IntradivisionalFirstSet or ScheduledGameType.IntradivisionalSecondSet);
				var intraconferenceCount = team.Count(m => m.GameType is ScheduledGameType.IntraconferenceFirstSet or ScheduledGameType.IntraconferenceSecondSet);
				var interconferenceCount = team.Count(m => m.GameType == ScheduledGameType.Interconference);
				var remainingIntraconferenceCount = team.Count(m => m.GameType is ScheduledGameType.RemainingIntraconferenceFirstSet or ScheduledGameType.RemainingIntraconferenceSecondSet);
				
				if (intradivisionalCount != 6 || intraconferenceCount != 4 || interconferenceCount != 4 || remainingIntraconferenceCount != 2)
				{
					allTeamsHaveCorrectNumberOfGamesByType = false;
					
					Log.Error("{Team} has {Intradivisional}/6 intradivisional games, {Intraconference}/4 intraconference games, {Interconference}/4 interconference games, and {RemainingIntraconference}/2 remaining intraconference games",
						team.Key.Name, intradivisionalCount, intraconferenceCount, interconferenceCount, remainingIntraconferenceCount);
				}
			}
			
			return allTeamsHaveCorrectNumberOfGamesByType
				? string.Empty
				: "Each team should have 6 intradivisional games, 4 intraconference games, 4 interconference games, and 2 remaining intraconference games";
		}
	}
}
