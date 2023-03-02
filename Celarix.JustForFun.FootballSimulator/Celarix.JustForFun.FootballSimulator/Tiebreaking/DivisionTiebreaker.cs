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
            var teamsInDivisionOrder = new Dictionary<ConferenceAndDivision, Team[]>();

            foreach (var division in teamsByDivision)
            {
                var tiedRangesInDivision = GetRangesContainingTies(division.Value);

                if (!tiedRangesInDivision.Any())
                {
                    teamsInDivisionOrder.Add(division.Key, division.Value
                        .Select(twp => GetTeamByName(twp.TeamName))
                        .ToArray());

                    continue;
                }

                foreach (var tiedRange in tiedRangesInDivision)
                {
                    var tiedTeams = division.Value[tiedRange];
                    
                    if (tiedRange.End.Value - tiedRange.Start.Value == 2)
                    {
                        
                    }
                    else
                    {
                        
                    }
                }
            }
        }

        private TeamWinPercentage[] BreakTwoWayTie(TeamWinPercentage a, TeamWinPercentage b)
        {
            // 1. A has a higher win percentage than B (false since we've got this far)
            // 2. A has a higher win percentage than B in games played between A and B (head-to-head)
            var headToHeadGames = GetAllGamesPlayedBetweenTeams(a.TeamName, b.TeamName);
            var headToHeadTeamAWinPercentage = GetWinPercentageForGamesWithTeam(a.TeamName, headToHeadGames);

            if (headToHeadTeamAWinPercentage != 0.000m)
            {
                switch (headToHeadTeamAWinPercentage)
                {
                    case > 0.500m:
                        return KeepOrder();
                    case < 0.500m:
                        return SwapOrder();
                }
            }

            // 3. A has a higher win percentage in games played within its own division
            var gamesPlayedInTeamADivision = GetAllGamesInDivisionPlayedByTeam(a.TeamName);
            var gamesPlayedInTeamBDivision = GetAllGamesInDivisionPlayedByTeam(b.TeamName);
            var teamADivisonRecord = GetWinPercentageForGamesWithTeam(a.TeamName, gamesPlayedInTeamADivision);
            var teamBDivisionRecord = GetWinPercentageForGamesWithTeam(b.TeamName, gamesPlayedInTeamBDivision);

            if (teamADivisonRecord > teamBDivisionRecord) { return KeepOrder(); }

            if (teamADivisonRecord < teamBDivisionRecord) { return SwapOrder(); }

            // 4. A has a higher win percentage against opponents that B also played
            var commonGames = GetCommonGamesBetweenTeams(a.TeamName, b.TeamName).ToArray();
            var teamACommonRecord = GetWinPercentageForGamesWithTeam(a.TeamName, commonGames);
            var teamBCommonRecord = GetWinPercentageForGamesWithTeam(b.TeamName, commonGames);

            if (teamACommonRecord > teamBCommonRecord) { return KeepOrder(); }

            if (teamACommonRecord < teamBDivisionRecord) { return SwapOrder(); }

            // 5. A has a higher win percentage against opponents within the conference
            var teamAConferenceGames = GetConferenceGamesForTeam(a.TeamName);
            var teamBConferenceGames = GetConferenceGamesForTeam(b.TeamName);
            var teamAConferenceRecord = GetWinPercentageForGamesWithTeam(a.TeamName, teamAConferenceGames);
            var teamBConferenceRecord = GetWinPercentageForGamesWithTeam(b.TeamName, teamBConferenceGames);

            if (teamAConferenceRecord > teamBConferenceRecord) { return KeepOrder(); }

            if (teamAConferenceRecord < teamBConferenceRecord) { return SwapOrder(); }

            // 6. A has a higher strength of victory than B
            var teamAStrengthOfVictory = GetStrengthOfVictoryForTeam(a.TeamName);
            var teamBStrengthOfVictory = GetStrengthOfVictoryForTeam(b.TeamName);

            if (teamAStrengthOfVictory > teamBStrengthOfVictory) { return KeepOrder(); }

            if (teamAStrengthOfVictory < teamBStrengthOfVictory) { return SwapOrder(); }

            // 7. A has a higher strength of schedule than B
            var teamAStrengthOfSchedule = GetStrengthOfScheduleForTeam(a.TeamName);
            var teamBStrengthOfSchedule = GetStrengthOfScheduleForTeam(b.TeamName);

            if (teamAStrengthOfSchedule > teamBStrengthOfSchedule) { return KeepOrder(); }

            if (teamAStrengthOfSchedule < teamBStrengthOfSchedule) { return SwapOrder(); }

            // 8. A has a higher ranking (points scored - points allowed) in its conference than B

            TeamWinPercentage[] KeepOrder()
            {
                return new[]
                {
                    a, b
                };
            }

            TeamWinPercentage[] SwapOrder()
            {
                return new[]
                {
                    a, b
                };
            }
        }

        private Team GetTeamByName(string teamName) => teams.Single(t => t.TeamName == teamName);

        private static IReadOnlyList<Range> GetRangesContainingTies(IReadOnlyList<TeamWinPercentage> winPercentages)
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

            return GetWinPercentageForGamesWithTeam(teamName, gamesPlayedByTeam);
        }

        private decimal GetWinPercentageForGamesWithTeam(string teamName, IEnumerable<GameRecord> games)
        {
            var wins = 0;
            var ties = 0;
            var gameCount = 0;

            foreach (var result in games.Where(g => GameHasTeamPlaying(g, teamName))
                         .Select(g => GetResultForTeamInGame(g, teamName)))
            {
                switch (result)
                {
                    case GameResultForTeam.Win:
                        wins += 1;
                        break;
                    case GameResultForTeam.Tie:
                        ties += 1;
                        break;
                    case GameResultForTeam.Loss:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                gameCount += 1;
            }

            return gameCount != 0
                ? (wins + (ties * 0.5m)) / gameCount
                : 0;
        }

        private GameResultForTeam GetResultForTeamInGame(GameRecord game, string teamName)
        {
            var opponentName = GetOpponentNameForGame(game, teamName);
            var teamScore = GetScoreForTeamInGame(game, teamName);
            var opponentScore = GetScoreForTeamInGame(game, opponentName);

            return teamScore > opponentScore
                ? GameResultForTeam.Win
                : teamScore == opponentScore
                    ? GameResultForTeam.Tie
                    : GameResultForTeam.Loss;
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

        private IEnumerable<GameRecord> GetAllGamesPlayedBetweenTeams(string teamAName, string teamBName) =>
            allSeasonGames.Where(g => GameHasTeamPlaying(g, teamAName) && GameHasTeamPlaying(g, teamBName));

        private IEnumerable<GameRecord> GetAllGamesInDivisionPlayedByTeam(string teamName) =>
            allSeasonGames.Where(g => GameHasTeamPlaying(g, teamName))
                .Where(g =>
                {
                    var team = GetTeamByName(teamName);
                    var opponentTeam = GetTeamByName(GetOpponentNameForGame(g, teamName));

                    return team.Conference == opponentTeam.Conference && team.Division == opponentTeam.Division;
                });

        private IEnumerable<GameRecord> GetCommonGamesBetweenTeams(string teamAName, string teamBName)
        {
            var teamAGames = allSeasonGames.Where(g => GameHasTeamPlaying(g, teamAName)).ToArray();
            var teamBGames = allSeasonGames.Where(g => GameHasTeamPlaying(g, teamBName)).ToArray();
            var teamAOpponentNames = teamAGames.Select(g => GetOpponentNameForGame(g, teamAName));
            var teamBOpponentNames = teamBGames.Select(g => GetOpponentNameForGame(g, teamBName));

            var commonOpponentNames = teamAOpponentNames.Where(n => teamBOpponentNames.Contains(n));

            return teamAGames
                .Where(g => commonOpponentNames.Contains(GetOpponentNameForGame(g, teamAName)))
                .Concat(teamBGames
                    .Where(g => commonOpponentNames.Contains(GetOpponentNameForGame(g, teamBName))));
        }

        private IEnumerable<GameRecord> GetConferenceGamesForTeam(string teamName)
        {
            var teamConference = GetTeamByName(teamName).Conference;
            
            return allSeasonGames.Where(g =>
                GameHasTeamPlaying(g, teamName) && GetTeamByName(GetOpponentNameForGame(g, teamName)).Conference == teamConference);
        }

        private decimal GetStrengthOfVictoryForTeam(string teamName)
        {
            var gamesWonByTeam = allSeasonGames.Where(g => GameHasTeamPlaying(g, teamName)
                && GetResultForTeamInGame(g, teamName) == GameResultForTeam.Win);
            var allDefeatedOpponentNames = gamesWonByTeam
                .Select(g => GetOpponentNameForGame(g, teamName))
                .ToArray();

            if (!allDefeatedOpponentNames.Any())
            {
                return 0m;
            }
            
            return allDefeatedOpponentNames
                    .Select(o => GetWinPercentageForTeam(o))
                    .Sum()
                / allDefeatedOpponentNames.Length;
        }

        private decimal GetStrengthOfScheduleForTeam(string teamName)
        {
            var gamesPlayedByTeam = allSeasonGames.Where(g => GameHasTeamPlaying(g, teamName));

            var allOpponentNames = gamesPlayedByTeam
                .Select(g => GetOpponentNameForGame(g, teamName))
                .ToArray();

            if (!allOpponentNames.Any()) { return 0m; }

            return allOpponentNames
                    .Select(o => GetWinPercentageForTeam(o))
                    .Sum()
                / allOpponentNames.Length;
        }

        private int GetPointsScoredMinusPointsAllowedForTeamInGame(GameRecord game, string teamName)
        {
            var teamScore = GetScoreForTeamInGame(game, teamName);
            var opponentScore = GetScoreForTeamInGame(game, GetOpponentNameForGame(game, teamName));

            return teamScore - opponentScore;
        }
        
        private static bool GameHasTeamPlaying(GameRecord game, string teamName) =>
            game.AwayTeam.TeamName == teamName
            || game.HomeTeam.TeamName == teamName;
    }
}
