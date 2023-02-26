using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;

namespace Celarix.JustForFun.FootballSimulator.Tiebreaking
{
    internal sealed class DivisionTiebreaker
    {
        private IReadOnlyList<Team> teams;
        private IReadOnlyList<GameRecord> allSeasonGames;
        private Random random;

        public DivisionTiebreaker(IEnumerable<Team> teams, IEnumerable<GameRecord> allSeasonGames)
        {
            this.teams = teams.ToList();
            this.allSeasonGames = allSeasonGames.ToList();
            random = new Random();
        }

        public Dictionary<ConferenceAndDivision, Team[]> GetTeamsInDivisionStandingsOrder()
        {
            var teamsByDivision = teams.GroupBy(t => new ConferenceAndDivision
            {
                Conference = t.Conference,
                Division = t.Division
            })
            .ToDictionary(g => g.Key,
                g => g.Select(t => new TeamWinPercentage
                    {
                        TeamName = t.TeamName,
                        WinPercentage = GetWinPercentageForTeam(t.TeamName)
                    })
                    .OrderBy(tw => tw.WinPercentage)
                    .ToArray());
        }

        private Team GetTeamByName(string teamName) => teams.Single(t => t.TeamName == teamName);

        private IReadOnlyList<Range> GetRangesContainingTies(IReadOnlyList<TeamWinPercentage> winPercentages)
        {
            var tiedRanges = new List<Range>();
            var currentTieRangeStart = -1;
            var currentTieRangeEnd = -1;
            TeamWinPercentage? lastTeam = null;

            for (var i = 0; i < winPercentages.Count; i++)
            {
                var teamWinPercentage = winPercentages[i];

                if (lastTeam?.WinPercentage == teamWinPercentage.WinPercentage)
                {
                    if (currentTieRangeStart == -1)
                    {
                        // Start of a new tie
                        currentTieRangeStart = i - 1;
                        currentTieRangeEnd = i;
                    }
                    else
                    {
                        // Nope, still tied
                        currentTieRangeEnd += 1;
                    }
                }
                else
                {
                    if (currentTieRangeStart != -1)
                    {
                        tiedRanges.Add(new Range(currentTieRangeStart, currentTieRangeEnd));
                    }

                    currentTieRangeStart = -1;
                    currentTieRangeEnd = -1;
                }

                lastTeam = teamWinPercentage;
            }

            return tiedRanges;
        }
        
        private decimal GetWinPercentageForTeam(string teamName)
        {
            var gamesPlayedByTeam = allSeasonGames
                .Where(g => g.HomeTeam.TeamName == teamName || g.AwayTeam.TeamName == teamName)
                .ToArray();

            var wins = 0;
            var ties = 0;

            foreach (var game in gamesPlayedByTeam)
            {
                var opponentName = GetOpponentNameForGame(game, teamName);
                var teamScore = GetScoreForTeamInGame(game, teamName);
                var opponentScore = GetScoreForTeamInGame(game, opponentName);

                if (teamScore > opponentScore)
                {
                    wins += 1;
                }
                else if (teamScore == opponentScore)
                {
                    ties += 1;
                }
            }

            return (wins + (ties * 0.5m)) / gamesPlayedByTeam.Length;
        }

        private int GetScoreForTeamInGame(GameRecord game, string teamName)
        {
            var teamIsAwayTeam = game.AwayTeam.TeamName == teamName;
            return game.QuarterBoxScores
                .Where(q => teamIsAwayTeam
                    ? q.Team == GameTeam.Away
                    : q.Team == GameTeam.Home)
                .Sum(q => q.Score);
        }

        private string GetOpponentNameForGame(GameRecord game, string selfTeamName)
        {
            var teamIsAwayTeam = game.AwayTeam.TeamName == selfTeamName;

            return teamIsAwayTeam
                ? game.HomeTeam.TeamName
                : game.AwayTeam.TeamName;
        }
    }
}
