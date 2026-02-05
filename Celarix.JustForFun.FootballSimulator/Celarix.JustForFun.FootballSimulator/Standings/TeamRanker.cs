using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    public sealed class TeamRanker
    {
        private readonly (Func<BasicTeamInfo, TieGroup, decimal> Tiebreaker, string Name)[] tiebreakers;

        private readonly List<BasicGameInfo> games;
        private readonly List<BasicTeamInfo> teams;

        public TeamRanker(IEnumerable<GameRecord> games,
            IEnumerable<Team> teams)
        {
            this.games = [.. games.Select(BasicGameInfo.FromGameRecord)];
            this.teams = [.. teams.Select(t => new BasicTeamInfo(t))];
            tiebreakers =
            [
                (HeadToHead, nameof(HeadToHead)),
                (Divisional, nameof(Divisional)),
                (CommonOpponents, nameof(CommonOpponents)),
                (Conference, nameof(Conference)),
                (StrengthOfVictory, nameof(StrengthOfVictory)),
                (StrengthOfSchedule, nameof(StrengthOfSchedule)),
                (PointDifferentialInConference, nameof(PointDifferentialInConference)),
                (PointDifferentialInLeague, nameof(PointDifferentialInLeague)),
                (CommonPointDifferential, nameof(CommonPointDifferential)),
                (OverallPointDifferential, nameof(OverallPointDifferential)),
                (Touchdowns, nameof(Touchdowns))
            ];

            Log.Verbose("Initialized TeamRanker with {GameCount} games and {TeamCount} teams",
                this.games.Count, this.teams.Count);
        }

        public IReadOnlyList<TeamViewRanking> RankTeams(IEnumerable<BasicTeamInfo> teamsToRank)
        {
            Log.Verbose("Ranking {TeamCount} teams",
                teamsToRank.Count());
            var tieGroups = RankTeamsIntoTieGroups(teamsToRank);
            var rankings = new List<TeamViewRanking>();
            var ranking = 1;
            foreach (var tieGroup in tieGroups)
            {
                foreach (var team in tieGroup.TiedTeams)
                {
                    Log.Verbose("{TeamNames} have {Rank}",
                        string.Join(", ", tieGroup.TiedTeams.Select(t => t.Name)),
                        ranking);
                    rankings.Add(new TeamViewRanking
                    {
                        BasicTeamInfo = team,
                        Ranking = ranking
                    });
                }
                ranking += 1;
            }

            return rankings;
        }

        public IReadOnlyList<TeamViewRanking> RankTeamsAndBreakCoinFlipTies(IEnumerable<BasicTeamInfo> teamsToRank, IRandom random)
        {
            Log.Verbose("Ranking {TeamCount} teams with coin flip tiebreakers",
                teamsToRank.Count());
            var tieGroups = RankTeamsIntoTieGroups(teamsToRank);
            var rankings = new List<TeamViewRanking>();
            var ranking = 1;
            foreach (var tieGroup in tieGroups)
            {
                var tiedTeams = tieGroup.TiedTeams;
                if (tiedTeams.Count > 1)
                {
                    // okay it's not a coin flip but it is a coin flip in spirit, darn it!
                    var randomOrder = random.ShuffleIntoNewList(tiedTeams);
                    Log.Verbose("Broke final tie for {TeamNames} with random order, assigning {Rank}",
                            string.Join(", ", tiedTeams.Select(t => t.Name)),
                            ranking);
                    foreach (var team in randomOrder)
                    {
                        Log.Verbose("Ranked {TeamName} at {Rank}", team.Name, ranking);
                        rankings.Add(new TeamViewRanking
                        {
                            BasicTeamInfo = team,
                            Ranking = ranking
                        });
                        ranking += 1;
                    }
                }
                else
                {
                    var team = tiedTeams[0];
                    Log.Verbose("Ranked {TeamName} at {Rank}", team.Name, ranking);
                    rankings.Add(new TeamViewRanking
                    {
                        BasicTeamInfo = team,
                        Ranking = ranking
                    });
                    ranking += 1;
                }
            }
            return rankings;
        }

        internal IReadOnlyList<TieGroup> RankTeamsIntoTieGroups(IEnumerable<BasicTeamInfo> teamsToRank)
        {
            var initialWinPercentages = teamsToRank
                .GroupBy(t => GetWinPercentageForTeamInGames(t, GetGamesWithTeam(t)))
                .OrderByDescending(g => g.Key);
            var tieGroups = initialWinPercentages.Select(g => new TieGroup(g)).ToList();

            foreach (var tiebreaker in tiebreakers)
            {
                var newTieGroups = new List<TieGroup>();
                foreach (var tieGroup in tieGroups)
                {
                    if (tieGroup.TiedTeams.Count <= 1)
                    {
                        newTieGroups.Add(tieGroup);
                    }
                    else
                    {
                        Log.Verbose("Applying tiebreaker {TiebreakerName} to teams: {TeamNames}",
                            tiebreaker.Name,
                            string.Join(", ", tieGroup.TiedTeams.Select(t => t.Name)));
                        var brokenGroups = BreakTieGroup(tieGroup, tiebreaker.Tiebreaker);
                        newTieGroups.AddRange(brokenGroups);

                        if (brokenGroups.Count > 1)
                        {
                            Log.Verbose("Tiebreaker {TiebreakerName} broke some ties, resulting in {GroupCount} groups:",
                                tiebreaker.Name,
                                brokenGroups.Count);
                            foreach (var group in brokenGroups)
                            {
                                Log.Verbose(" - {TeamNames}",
                                    string.Join(", ", group.TiedTeams.Select(t => t.Name)));
                            }
                        }
                    }
                }
                tieGroups = newTieGroups;
            }

            return tieGroups;
        }

        internal static List<TieGroup> BreakTieGroup(TieGroup tieGroup, Func<BasicTeamInfo, TieGroup, decimal> keySelector)
        {
            var groups = tieGroup.TiedTeams.GroupBy(t => keySelector(t, tieGroup));
            return [.. groups.Select(g => new TieGroup(g))];
        }

        // Tiebreakers
        internal decimal HeadToHead(BasicTeamInfo team, TieGroup tieGroup)
        {
            var otherTeams = tieGroup.TiedTeams.Where(t => !t.Equals(team));
            var headToHeadTeamGames = GetGamesWithTeam(team)
                .Where(g => GameHasTeam(g, otherTeams));
            return GetWinPercentageForTeamInGames(team, headToHeadTeamGames);
        }

        internal decimal Divisional(BasicTeamInfo team, TieGroup tieGroup)
        {
            var divisionalTeamGames = GetGamesWithTeam(team)
                .Where(g => g.IsDivisionGame);
            return GetWinPercentageForTeamInGames(team, divisionalTeamGames);
        }

        internal decimal CommonOpponents(BasicTeamInfo team, TieGroup tieGroup)
        {
            var teamGames = tieGroup.TiedTeams
                .ToDictionary(t => t, t => GetGamesWithTeam(t).ToArray());
            var opponents = teamGames.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value.Select(g => g.OpponentOf(kvp.Key)).Distinct().ToArray());
            var commonOpponents = Helpers.IntersectAll(opponents.Select(kvp => kvp.Value));
            if (commonOpponents.Count == 0) { return 0m; }
            Log.Verbose("Common opponents for {TeamNames}: {CommonOpponentNames}",
                string.Join(", ", tieGroup.TiedTeams.Select(t => t.Name)),
                string.Join(", ", commonOpponents.Select(t => t.Name)));

            var teamCommonGames = GetGamesWithTeam(team)
                .Where(g => GameHasTeam(g, commonOpponents));
            return GetWinPercentageForTeamInGames(team, teamCommonGames);
        }

        internal decimal Conference(BasicTeamInfo team, TieGroup tieGroup)
        {
            var conferenceTeamGames = GetGamesWithTeam(team)
                .Where(g => g.IsConferenceGame);
            return GetWinPercentageForTeamInGames(team, conferenceTeamGames);
        }

        internal decimal StrengthOfVictory(BasicTeamInfo team, TieGroup _)
        {
            var teamGames = GetGamesWithTeam(team)
                .Where(g => !g.Tie && g.WinningTeam!.Equals(team));
            var defeatedTeams = teamGames
                .Select(g => g.OpponentOf(team))
                .Distinct();
            if (!defeatedTeams.Any()) { return 0m; }

            Log.Verbose("Defeated opponents for {TeamName}: {DefeatedTeamNames}",
                team.Name,
                string.Join(", ", defeatedTeams.Select(t => t.Name)));

            decimal totalWinPercentage = 0m;
            foreach (var defeatedTeam in defeatedTeams)
            {
                var defeatedTeamGames = GetGamesWithTeam(defeatedTeam);
                var defeatedTeamWinPercentage = GetWinPercentageForTeamInGames(defeatedTeam, defeatedTeamGames);
                totalWinPercentage += defeatedTeamWinPercentage;
            }
            return totalWinPercentage / defeatedTeams.Count();
        }

        internal decimal StrengthOfSchedule(BasicTeamInfo team, TieGroup _)
        {
            var teamGames = GetGamesWithTeam(team);
            var opponents = teamGames
                .Select(g => g.OpponentOf(team))
                .Distinct();
            if (!opponents.Any()) { return 0m; }

            Log.Verbose("Opponents for {TeamName}: {OpponentNames}",
                team.Name,
                string.Join(", ", opponents.Select(t => t.Name)));

            decimal totalWinPercentage = 0m;
            foreach (var opponent in opponents)
            {
                var opponentGames = GetGamesWithTeam(opponent);
                var opponentWinPercentage = GetWinPercentageForTeamInGames(opponent, opponentGames);
                totalWinPercentage += opponentWinPercentage;
            }
            return totalWinPercentage / opponents.Count();
        }

        internal decimal PointDifferentialInConference(BasicTeamInfo team, TieGroup _)
        {
            var teamsByConference = teams.GroupBy(t => t.Conference);
            var pointsForRankings = teamsByConference.ToDictionary(
                g => g.Key, GetPointsForRanking);
            var pointsAgainstRankings = teamsByConference.ToDictionary(
                g => g.Key, GetPointsAgainstRanking);

            var teamPointsForRanking = pointsForRankings[team.Conference]
                .IndexOf(team) + 1;
            var teamPointsAgainstRanking = pointsAgainstRankings[team.Conference]
                .IndexOf(team) + 1;
            Log.Verbose("Team {TeamName} is ranked #{PointsForRank} in points for and #{PointsAgainstRank} in points against in conference {Conference} (combined {Combined})",
                team.Name,
                teamPointsForRanking,
                teamPointsAgainstRanking,
                team.Conference,
                teamPointsForRanking + teamPointsAgainstRanking);
            return teamPointsForRanking + teamPointsAgainstRanking;
        }

        internal decimal PointDifferentialInLeague(BasicTeamInfo team, TieGroup _)
        {
            var pointsForRankings = GetPointsForRanking(teams);
            var pointsAgainstRankings = GetPointsAgainstRanking(teams);

            var teamPointsForRanking = pointsForRankings.IndexOf(team) + 1;
            var teamPointsAgainstRanking = pointsAgainstRankings.IndexOf(team) + 1;
            Log.Verbose("Team {TeamName} is ranked #{PointsForRank} in points for and #{PointsAgainstRank} in points against in league (combined {Combined})",
                team.Name,
                teamPointsForRanking,
                teamPointsAgainstRanking,
                teamPointsForRanking + teamPointsAgainstRanking);
            return teamPointsForRanking + teamPointsAgainstRanking;
        }

        internal decimal CommonPointDifferential(BasicTeamInfo team, TieGroup tieGroup)
        {
            var teamGames = tieGroup.TiedTeams
                .ToDictionary(t => t, t => GetGamesWithTeam(t).ToArray());
            var opponents = teamGames.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value.Select(g => g.OpponentOf(kvp.Key)).Distinct().ToArray());
            var commonOpponents = Helpers.IntersectAll(opponents.Select(kvp => kvp.Value));
            if (commonOpponents.Count == 0) { return 0m; }
            
            Log.Verbose("Common opponents for {TeamNames}: {CommonOpponentNames}",
                string.Join(", ", tieGroup.TiedTeams.Select(t => t.Name)),
                string.Join(", ", commonOpponents.Select(t => t.Name)));

            var teamCommonGames = GetGamesWithTeam(team)
                .Where(g => GameHasTeam(g, commonOpponents));
            return teamCommonGames.Sum(g => g.PointsFor(team) - g.PointsAgainst(team));
        }

        internal decimal OverallPointDifferential(BasicTeamInfo team, TieGroup _)
        {
            var teamGames = GetGamesWithTeam(team);
            int overallPointDifferential = teamGames.Sum(g => g.PointsFor(team) - g.PointsAgainst(team));
            Log.Verbose("{TeamName} overall point differential: {PointDifferential}",
                team.Name,
                overallPointDifferential);
            return overallPointDifferential;
        }

        internal decimal Touchdowns(BasicTeamInfo team, TieGroup _)
        {
            var teamGames = GetGamesWithTeam(team);
            int touchdowns = teamGames.Sum(g => g.TouchdownsFor(team));
            Log.Verbose("{TeamName} touchdowns: {Touchdowns}",
                team.Name,
                touchdowns);
            return touchdowns;
        }

        // Helpers
        internal IEnumerable<BasicGameInfo> GetGamesWithTeam(BasicTeamInfo team)
        {
            foreach (var game in games)
            {
                if (game.HomeTeam.Equals(team) || game.AwayTeam.Equals(team))
                {
                    yield return game;
                }
            }
        }

        internal static decimal GetWinPercentageForTeamInGames(BasicTeamInfo team,
            IEnumerable<BasicGameInfo> games)
        {
            int wins = 0;
            int ties = 0;
            int totalGames = 0;
            foreach (var game in games)
            {
                totalGames++;
                if (game.Tie)
                {
                    ties += 1;
                }
                else if (game.WinningTeam!.Equals(team))
                {
                    wins += 1;
                }
            }

            if (totalGames == 0)
            {
                return 0;
            }

            decimal winPercentage = (wins + (ties * 0.5m)) / totalGames;
            Log.Verbose("{TeamName} in {TotalGames} games: {Wins}-{Losses}-{Ties} ({WinPercentage:P2})",
                team.Name,
                totalGames,
                wins,
                totalGames - wins - ties,
                ties,
                winPercentage);
            return winPercentage;
        }

        internal static bool GameHasTeam(BasicGameInfo game,
            BasicTeamInfo team) =>
            game.HomeTeam.Equals(team) || game.AwayTeam.Equals(team);

        internal static bool GameHasTeam(BasicGameInfo game,
            IEnumerable<BasicTeamInfo> teams)
        {
            foreach (var team in teams)
            {
                if (GameHasTeam(game, team))
                {
                    return true;
                }
            }
            return false;
        }

        internal IReadOnlyList<BasicTeamInfo> GetPointsForRanking(IEnumerable<BasicTeamInfo> teams)
        {
            return teams
                .Select(t => new
                {
                    Team = t,
                    Points = GetGamesWithTeam(t).Sum(g => g.PointsFor(t))
                })
                .OrderByDescending(tp => tp.Points)
                .Select(tp => tp.Team)
                .ToList()
                .AsReadOnly();
        }

        internal IReadOnlyList<BasicTeamInfo> GetPointsAgainstRanking(IEnumerable<BasicTeamInfo> teams)
        {
            return teams
                .Select(t => new
                {
                    Team = t,
                    Points = GetGamesWithTeam(t).Sum(g => g.PointsAgainst(t))
                })
                .OrderByDescending(tp => tp.Points)
                .Select(tp => tp.Team)
                .ToList()
                .AsReadOnly();
        }
    }
}
