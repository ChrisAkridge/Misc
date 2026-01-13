using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Serilog;

namespace Celarix.JustForFun.FootballSimulator.Tiebreaking
{
    internal sealed class DivisionTiebreaker
    {
        private IReadOnlyList<Team> teams;
        private IReadOnlyList<GameRecord> allSeasonGames;
        private IRandom random;

        public DivisionTiebreaker(IEnumerable<Team> teams, IEnumerable<GameRecord> allSeasonGames, IRandom random)
        {
            this.teams = teams.ToList();
            this.allSeasonGames = allSeasonGames.ToList();
            this.random = random;
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
            var comparer = new DelegateComparer<TeamWinPercentage>((a, b) => CompareTeams(a!, b!));

            foreach (var division in teamsByDivision)
            {
                var winPercentagesInDivisionOrder = division.Value
                    .OrderBy(p => p, comparer);
                teamsInDivisionOrder.Add(division.Key, winPercentagesInDivisionOrder
                    .Select(p => teams.Single(t => t.TeamName == p.TeamName))
                    .ToArray());
            }

            return teamsInDivisionOrder;
        }

        public Dictionary<BasicTeamInfo, int> GetTeamDivisionRankings()
        {
            var standingsOrder = GetTeamsInDivisionStandingsOrder();
            var rankings = new Dictionary<BasicTeamInfo, int>();

            foreach (var kvp in standingsOrder)
            {
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    var team = kvp.Value[i];
                    rankings[new BasicTeamInfo(team.TeamName, team.Conference, team.Division)] = i + 1;
                }
            }

            return rankings;
        }

        public IReadOnlyList<BasicTeamInfo> RankTeams(IEnumerable<BasicTeamInfo> teams)
        {
            var winPercentages = teams.ToDictionary(t => t,
                t => new TeamWinPercentage
                {
                    TeamName = t.Name,
                    WinPercentage = GetWinPercentageForTeam(t.Name)
                });
            var comparer = new DelegateComparer<TeamWinPercentage>((a, b) => CompareTeams(a!, b!));
            var teamsInOrder = winPercentages.OrderBy(kvp => kvp.Value, comparer);
            return [.. teamsInOrder.Select(kvp => kvp.Key)];
        }

        private int CompareTeams(TeamWinPercentage a, TeamWinPercentage b)
        {
            // 1. A has a higher win percentage than B
            if (a.WinPercentage > b.WinPercentage) { return -1; }

            // 2. A has a higher win percentage than B in games played between A and B (head-to-head)
            var headToHeadGames = GetAllGamesPlayedBetweenTeams(a.TeamName, b.TeamName);
            var headToHeadTeamAWinPercentage = GetWinPercentageForGamesWithTeam(a.TeamName, headToHeadGames);

            if (headToHeadTeamAWinPercentage != 0.000m)
            {
                switch (headToHeadTeamAWinPercentage)
                {
                    case > 0.500m:
                        Log.Verbose("{A} wins tiebreaker over {B} by head-to-head record.", a.TeamName, b.TeamName);
                        return -1;
                    case < 0.500m:
                        Log.Verbose("{B} wins tiebreaker over {A} by head-to-head record.", b.TeamName, a.TeamName);
                        return 1;
                }
            }

            // 3. A has a higher win percentage in games played within its own division
            var gamesPlayedInTeamADivision = GetAllGamesInDivisionPlayedByTeam(a.TeamName);
            var gamesPlayedInTeamBDivision = GetAllGamesInDivisionPlayedByTeam(b.TeamName);
            var teamADivisonRecord = GetWinPercentageForGamesWithTeam(a.TeamName, gamesPlayedInTeamADivision);
            var teamBDivisionRecord = GetWinPercentageForGamesWithTeam(b.TeamName, gamesPlayedInTeamBDivision);

            if (teamADivisonRecord > teamBDivisionRecord)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by higher win percentage in division games.", a.TeamName, b.TeamName);
                return -1;
            }

            if (teamADivisonRecord < teamBDivisionRecord)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher win percentage in division games.", b.TeamName, a.TeamName);
                return 1;
            }

            // 4. A has a higher win percentage against opponents that B also played
            var commonGames = GetCommonGamesBetweenTeams(a.TeamName, b.TeamName).ToArray();
            var teamACommonRecord = GetWinPercentageForGamesWithTeam(a.TeamName, commonGames);
            var teamBCommonRecord = GetWinPercentageForGamesWithTeam(b.TeamName, commonGames);

            if (teamACommonRecord > teamBCommonRecord)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by higher win percentage in common games.", a.TeamName, b.TeamName);
                return -1;
            }

            if (teamACommonRecord < teamBDivisionRecord)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher win percentage in common games.", b.TeamName, a.TeamName);
                return 1;
            }

            // 5. A has a higher win percentage against opponents within the conference
            var teamAConferenceGames = GetConferenceGamesForTeam(a.TeamName);
            var teamBConferenceGames = GetConferenceGamesForTeam(b.TeamName);
            var teamAConferenceRecord = GetWinPercentageForGamesWithTeam(a.TeamName, teamAConferenceGames);
            var teamBConferenceRecord = GetWinPercentageForGamesWithTeam(b.TeamName, teamBConferenceGames);

            if (teamAConferenceRecord > teamBConferenceRecord)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by higher win percentage in conference games.", a.TeamName, b.TeamName);
                return -1;
            }

            if (teamAConferenceRecord < teamBConferenceRecord)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher win percentage in conference games.", b.TeamName, a.TeamName);
                return 1;
            }

            // 6. A has a higher strength of victory than B
            var teamAStrengthOfVictory = GetStrengthOfVictoryForTeam(a.TeamName);
            var teamBStrengthOfVictory = GetStrengthOfVictoryForTeam(b.TeamName);

            if (teamAStrengthOfVictory > teamBStrengthOfVictory)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by higher strength of victory.", a.TeamName, b.TeamName);
                return -1;
            }

            if (teamAStrengthOfVictory < teamBStrengthOfVictory)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher strength of victory.", b.TeamName, a.TeamName);
                return 1;
            }

            // 7. A has a higher strength of schedule than B
            var teamAStrengthOfSchedule = GetStrengthOfScheduleForTeam(a.TeamName);
            var teamBStrengthOfSchedule = GetStrengthOfScheduleForTeam(b.TeamName);

            if (teamAStrengthOfSchedule > teamBStrengthOfSchedule)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by higher strength of schedule.", a.TeamName, b.TeamName);
                return -1;
            }

            if (teamAStrengthOfSchedule < teamBStrengthOfSchedule)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher strength of schedule.", b.TeamName, a.TeamName);
                return 1;
            }

            // 8. A has a higher ranking (points scored - points allowed) in its conference than B
            var teamAConferenceTeams = GetTeamsInConferenceOfTeam(a.TeamName);
            var teamBConferenceTeams = GetTeamsInConferenceOfTeam(b.TeamName);

            var conferenceAPointsScoredMinusPointsAllowedRankings =
                GetTeamNamesInPointsScoredMinusPointsAllowedOrder(teamAConferenceTeams);
            var conferenceBPointsScoredMinusPointsAllowedRankings =
                GetTeamNamesInPointsScoredMinusPointsAllowedOrder(teamBConferenceTeams);
            var teamAPointsScoredMinusPointsAllowedRanking =
                conferenceAPointsScoredMinusPointsAllowedRankings.IndexOf(a.TeamName);
            var teamBPointsScoredMinusPointsAllowedRanking =
                conferenceBPointsScoredMinusPointsAllowedRankings.IndexOf(b.TeamName);
            
            if (teamAPointsScoredMinusPointsAllowedRanking < teamBPointsScoredMinusPointsAllowedRanking)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by having a higher point differential.", a.TeamName, b.TeamName);
                return -1;
            }

            if (teamAPointsScoredMinusPointsAllowedRanking > teamBPointsScoredMinusPointsAllowedRanking)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher a higher point differential.", b.TeamName, a.TeamName);
                return 1;
            }

            // 9. A has a higher ranking (points scored - points allowed) against all teams than B
            var allPointsScoredMinusPointsAllowedRankings =
                GetTeamNamesInPointsScoredMinusPointsAllowedOrder(teams.Select(t => t.TeamName).ToArray());
            teamAPointsScoredMinusPointsAllowedRanking = allPointsScoredMinusPointsAllowedRankings.IndexOf(a.TeamName);
            teamBPointsScoredMinusPointsAllowedRanking = allPointsScoredMinusPointsAllowedRankings.IndexOf(b.TeamName);
            
            if (teamAPointsScoredMinusPointsAllowedRanking < teamBPointsScoredMinusPointsAllowedRanking)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by having a higher point differential against all teams.", a.TeamName, b.TeamName);
                return -1;
            }

            if (teamAPointsScoredMinusPointsAllowedRanking > teamBPointsScoredMinusPointsAllowedRanking)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher a higher point differential against all teams.", b.TeamName, a.TeamName);
                return 1;
            }

            // 10. A has a higher (points scored - points allowed) against opponents that B also played
            var commonOpponentNames = GetCommonOpponentNamesExceptEachOther(commonGames, a.TeamName, b.TeamName).ToArray();
            var pointsScoredMinusPointsAllowedForCommonAGames = commonGames
                .Where(g => GameHasTeamPlaying(g, a.TeamName)
                    && commonOpponentNames.Contains(GetOpponentNameForGame(g, a.TeamName)))
                .Sum(g => GetPointsScoredMinusPointsAllowedForTeamInGame(g, a.TeamName));
            var pointsScoredMinusPointsAllowedForCommonBGames = commonGames
                .Where(g => GameHasTeamPlaying(g, b.TeamName)
                    && commonOpponentNames.Contains(GetOpponentNameForGame(g, b.TeamName)))
                .Sum(g => GetPointsScoredMinusPointsAllowedForTeamInGame(g, b.TeamName));
            
            if (pointsScoredMinusPointsAllowedForCommonAGames > pointsScoredMinusPointsAllowedForCommonBGames)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by having a higher point differential in common games.", a.TeamName, b.TeamName);
                return -1;
            }

            if (pointsScoredMinusPointsAllowedForCommonAGames < pointsScoredMinusPointsAllowedForCommonBGames)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher a higher point differential in common games.", b.TeamName, a.TeamName);
                return 1;
            }

            // 11. A has a higher (points scored - points allowed) in all of its game than B
            var pointsScoredMinusPointsAllowedForAllAGames = allSeasonGames
                .Where(g => GameHasTeamPlaying(g, a.TeamName))
                .Sum(g => GetPointsScoredMinusPointsAllowedForTeamInGame(g, a.TeamName));
            var pointsScoredMinusPointsAllowedForAllBGames = allSeasonGames
                .Where(g => GameHasTeamPlaying(g, b.TeamName))
                .Sum(g => GetPointsScoredMinusPointsAllowedForTeamInGame(g, b.TeamName));
            
            if (pointsScoredMinusPointsAllowedForAllAGames > pointsScoredMinusPointsAllowedForAllBGames)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by having a higher point differential in all games.", a.TeamName, b.TeamName);
                return -1;
            }

            if (pointsScoredMinusPointsAllowedForAllAGames < pointsScoredMinusPointsAllowedForAllBGames)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by higher a higher point differential in all games.", b.TeamName, a.TeamName);
                return 1;
            }

            // 12. A has scored more touchdowns than B
            var touchdownsScoredByA = GetTouchdownCountForTeamInGames(allSeasonGames
                .Where(g => GameHasTeamPlaying(g, a.TeamName)), a.TeamName);
            var touchdownsScoredByB = GetTouchdownCountForTeamInGames(allSeasonGames
                .Where(g => GameHasTeamPlaying(g, b.TeamName)), b.TeamName);

            if (touchdownsScoredByA > touchdownsScoredByB)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by scoring more touchdowns.", a.TeamName, b.TeamName);
                return -1;
            }

            if (touchdownsScoredByA < touchdownsScoredByB)
            {
                Log.Verbose("{B} wins tiebreaker over {A} by scoring more touchdowns.", b.TeamName, a.TeamName);
                return 1;
            }

            // 13. A coin toss comes up heads
            if (random.NextDouble() < 0.5d)
            {
                Log.Verbose("{A} wins tiebreaker over {B} by coin toss.", a.TeamName, b.TeamName);
                return -1;
            }
            else
            {
                Log.Verbose("{B} wins tiebreaker over {A} by coin toss.", b.TeamName, a.TeamName);
                return 1;
            }
        }

        private Team GetTeamByName(string teamName) => teams.Single(t => t.TeamName == teamName);

        private decimal GetWinPercentageForTeam(string teamName)
        {
            var gamesPlayedByTeam = allSeasonGames
                .Where(g => g.HomeTeam.TeamName == teamName || g.AwayTeam.TeamName == teamName)
                .ToArray();

            return GetWinPercentageForGamesWithTeam(teamName, gamesPlayedByTeam);
        }

        private static decimal GetWinPercentageForGamesWithTeam(string teamName, IEnumerable<GameRecord> games)
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

        private static GameResultForTeam GetResultForTeamInGame(GameRecord game, string teamName)
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

        private static int GetScoreForTeamInGame(GameRecord game, string teamName)
        {
            var teamIsAwayTeam = game.AwayTeam.TeamName == teamName;
            return game.QuarterBoxScores
                .Where(q => teamIsAwayTeam
                    ? q.Team == GameTeam.Away
                    : q.Team == GameTeam.Home)
                .Sum(q => q.Score);
        }

        private static string GetOpponentNameForGame(GameRecord game, string selfTeamName)
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
                    .Where(g => commonOpponentNames.Contains(GetOpponentNameForGame(g, teamBName))))
                .ToArray();
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

        private static int GetPointsScoredMinusPointsAllowedForTeamInGame(GameRecord game, string teamName)
        {
            var teamScore = GetScoreForTeamInGame(game, teamName);
            var opponentScore = GetScoreForTeamInGame(game, GetOpponentNameForGame(game, teamName));

            return teamScore - opponentScore;
        }

        private string[] GetTeamNamesInPointsScoredMinusPointsAllowedOrder(string[] unsortedTeamNames)
        {
            return unsortedTeamNames
                .Select(n =>
                {
                    var gamesPlayedByTeamAgainstProvidedTeams = allSeasonGames.Where(g => GameHasTeamPlaying(g, n)
                        && unsortedTeamNames.Contains(GetOpponentNameForGame(g, n)));
                    return new
                    {
                        TeamName = n,
                        PointsScoredMinusPointsAllowed = gamesPlayedByTeamAgainstProvidedTeams
                            .Sum(g => GetPointsScoredMinusPointsAllowedForTeamInGame(g, n))
                    };
                })
                .OrderByDescending(np => np.PointsScoredMinusPointsAllowed)
                .Select(np => np.TeamName)
                .ToArray();
        }

        private string[] GetTeamsInConferenceOfTeam(string teamName)
        {
            var teamConference = GetTeamByName(teamName).Conference;
            return teams
                .Where(t => t.Conference == teamConference)
                .Select(t => t.TeamName)
                .ToArray();
        }

        private static IEnumerable<string> GetCommonOpponentNamesExceptEachOther(IEnumerable<GameRecord> games, string aTeamName,
            string bTeamName) =>
            games.Where(g => !GameHasTeamPlaying(g, aTeamName) || !GameHasTeamPlaying(g, bTeamName))
                .Select(game =>
                    GameHasTeamPlaying(game, aTeamName)
                        ? GetOpponentNameForGame(game, aTeamName)
                        : GetOpponentNameForGame(game, bTeamName));

        private static int GetTouchdownCountForTeamInGames(IEnumerable<GameRecord> games, string teamName) =>
            games.Sum(g => g.TeamDriveRecords
                .Where(d => d.Team == GetHomeOrAwayForTeamInGame(g, teamName))
                .Count(d => d.Result == DriveResult.TouchdownNoXP));

        private static bool GameHasTeamPlaying(GameRecord game, string teamName) =>
            game.AwayTeam.TeamName == teamName
            || game.HomeTeam.TeamName == teamName;

        private static GameTeam GetHomeOrAwayForTeamInGame(GameRecord game, string teamName) =>
            game.HomeTeam.TeamName == teamName
                ? GameTeam.Home
                : GameTeam.Away;
    }
}
