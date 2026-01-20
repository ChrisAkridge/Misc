using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    internal sealed class TeamRanker
    {
        private readonly Func<BasicTeamInfo, TieGroup, decimal>[] tiebreakers;

        private readonly List<BasicGameInfo> games;
        private readonly List<BasicTeamInfo> teams;

        public TeamRanker(IEnumerable<GameRecord> games,
            IEnumerable<Team> teams)
        {
            this.games = [.. games.Select(BasicGameInfo.FromGameRecord)];
            this.teams = [.. teams.Select(t => new BasicTeamInfo(t))];
            tiebreakers =
            [
                HeadToHead,
                Divisional,
                CommonOpponents,
                Conference,
                StrengthOfVictory,
                StrengthOfSchedule,
                PointDifferentialInConference,
                PointDifferentialInLeague,
                CommonPointDifferential,
                OverallPointDifferential,
                Touchdowns
            ];
        }

        public IReadOnlyList<TeamViewRanking> RankTeams(IEnumerable<BasicTeamInfo> teamsToRank)
        {
            var initialWinPercentages = teamsToRank.GroupBy(t => GetWinPercentageForTeamInGames(t, GetGamesWithTeam(t)));
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
                        var brokenGroups = BreakTieGroup(tieGroup, tiebreaker);
                        newTieGroups.AddRange(brokenGroups);
                    }
                }
                tieGroups = newTieGroups;
            }

            var rankings = new List<TeamViewRanking>();
            var ranking = 1;
            foreach (var tieGroup in tieGroups)
            {
                foreach (var team in tieGroup.TiedTeams)
                {
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

        internal decimal Divisional(BasicTeamInfo team, TieGroup _)
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

            var teamCommonGames = GetGamesWithTeam(team)
                .Where(g => GameHasTeam(g, commonOpponents));
            return GetWinPercentageForTeamInGames(team, teamCommonGames);
        }

        internal decimal Conference(BasicTeamInfo team, TieGroup _)
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
            return teamPointsForRanking + teamPointsAgainstRanking;
        }

        internal decimal PointDifferentialInLeague(BasicTeamInfo team, TieGroup _)
        {
            var pointsForRankings = GetPointsForRanking(teams);
            var pointsAgainstRankings = GetPointsAgainstRanking(teams);

            var teamPointsForRanking = pointsForRankings.IndexOf(team) + 1;
            var teamPointsAgainstRanking = pointsAgainstRankings.IndexOf(team) + 1;
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

            var teamCommonGames = GetGamesWithTeam(team)
                .Where(g => GameHasTeam(g, commonOpponents));
            return teamCommonGames.Sum(g => g.PointsFor(team) - g.PointsAgainst(team));
        }

        internal decimal OverallPointDifferential(BasicTeamInfo team, TieGroup _)
        {
            var teamGames = GetGamesWithTeam(team);
            return teamGames.Sum(g => g.PointsFor(team) - g.PointsAgainst(team));
        }

        internal decimal Touchdowns(BasicTeamInfo team, TieGroup _)
        {
            var teamGames = GetGamesWithTeam(team);
            return teamGames.Sum(g => g.TouchdownsFor(team));
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
                if (game.Tie)
                {
                    ties += 1;
                }
                totalGames++;
                if (game.WinningTeam!.Equals(team))
                {
                    wins += 1;
                }
            }

            if (totalGames == 0)
            {
                return 0;
            }

            return (wins + (ties * 0.5m)) / totalGames;
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
