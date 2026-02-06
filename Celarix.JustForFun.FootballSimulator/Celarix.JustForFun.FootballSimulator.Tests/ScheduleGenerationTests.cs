using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests
{
    public sealed class ScheduleGenerationTests
    {
        internal record TestSchedule(
            List<GameRecord> Games,
            IReadOnlyList<TeamScheduleDiagnostics> Diagnostics
        );

        private TestSchedule GenerateScheduleForYear(int year)
        {
            var context = new FootballContext();
            if (!context.Database.CanConnect())
            {
                throw new InvalidOperationException("Database does not exist. Cannot run tests.");
            }
            var teams = context.Teams.ToList();
            var dataTeams = teams.ToDictionary(t => new BasicTeamInfo(t.TeamName, t.Conference, t.Division), t => t);
            var scheduleGenerator = new ScheduleGenerator3(dataTeams.Keys.ToArray(), new RandomFactory());
            var schedule = scheduleGenerator.GenerateScheduleForYear(year, dataTeams, null, null, out var diagnostics);
            return new(schedule, diagnostics);
        }

        [Theory]
        [InlineData(2014)]
        [InlineData(2015)]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        public void SchedulesHaveCorrectNumberOfPreseasonGames(int year)
        {
            var schedule = GenerateScheduleForYear(year);
            var preseasonGames = schedule.Games.Where(gr => gr.GameType == GameType.Preseason).ToArray();
            Assert.Equal(80, preseasonGames.Length);
        }


        [Theory]
        [InlineData(2014)]
        [InlineData(2015)]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        public void EachTeamPlaysExactlyFourPreseasonGames(int year)
        {
            var schedule = GenerateScheduleForYear(year);
            var context = new FootballContext();
            var teams = context.Teams.ToList();

            foreach (var teamEntity in teams)
            {
                var teamInfo = ToBasicTeamInfo(teamEntity);

                var preseasonCount = schedule.Games
                    .Where(gr => gr.GameType == GameType.Preseason && TeamPlaysInGame(teamInfo, gr))
                    .Count();

                Assert.Equal(4, preseasonCount);
            }
        }

        [Theory]
        [InlineData(2014)]
        [InlineData(2015)]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        public void SchedulesHaveCorrectNumberOfRegularSeasonGames(int year)
        {
            var schedule = GenerateScheduleForYear(year);
            var regularSeasonGames = schedule.Games.Where(gr => gr.GameType == GameType.RegularSeason).ToArray();
            Assert.Equal(320, regularSeasonGames.Length);
        }

        [Theory]
        [InlineData(2014)]
        [InlineData(2015)]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        public void EachTeamHasProperIntradivisionGames(int year)
        {
            var schedule = GenerateScheduleForYear(year);
            var context = new FootballContext();
            var teams = context.Teams.ToList();
            var successfulTeams = 0;

            foreach (var teamEntity in teams)
            {
                var teamInfo = ToBasicTeamInfo(teamEntity);

                // Determine division opponents: same conference and division, different team name
                var divisionOpponents = teams
                    .Where(t => t.Conference == teamEntity.Conference
                                && t.Division == teamEntity.Division
                                && t.TeamName != teamEntity.TeamName)
                    .Select(t => new BasicTeamInfo(t.TeamName, t.Conference, t.Division))
                    .ToList();

                // Sanity check: there should be exactly 3 division opponents
                Assert.Equal(3, divisionOpponents.Count);

                var diagnostic = schedule.Diagnostics.FirstOrDefault(d => d.Team!.Name == teamInfo.Name);
                Assert.NotNull(diagnostic);
                var intradivisionGames = diagnostic
                    .GetOpponentsByGameType(ScheduledGameType.IntradivisionalFirstSet)
                    .Concat(diagnostic.GetOpponentsByGameType(ScheduledGameType.IntradivisionalSecondSet))
                    .ToArray();

                // Assert that there are six opponents
                Assert.Equal(6, intradivisionGames.Length);
                // Assert that each division opponent appears exactly twice
                foreach (var opponent in divisionOpponents)
                {
                    var occurrences = intradivisionGames.Count(t => t.Name == opponent.Name);
                    Assert.Equal(2, occurrences);
                }

                successfulTeams += 1;
            }
        }

        [Theory]
        [InlineData(2014)]
        [InlineData(2015)]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        public void IntraconferenceGamesAreCorrect(int year)
        {
            var schedule = GenerateScheduleForYear(year);
            var context = new FootballContext();
            var teams = context.Teams.ToList();

            foreach (var teamEntity in teams)
            {
                var teamInfo = ToBasicTeamInfo(teamEntity);

                // Determine division opponents: same conference and division, different team name
                var divisionOpponents = teams
                    .Where(t => t.Conference == teamEntity.Conference
                                && t.Division == teamEntity.Division
                                && t.TeamName != teamEntity.TeamName)
                    .Select(t => new BasicTeamInfo(t.TeamName, t.Conference, t.Division))
                    .ToList();

                // Sanity check for three division opponents
                Assert.Equal(3, divisionOpponents.Count);

                var diagnostic = schedule.Diagnostics.FirstOrDefault(d => d.Team!.Name == teamInfo.Name);
                Assert.NotNull(diagnostic);

                var intraconferenceOpponents = diagnostic
                    .GetOpponentsByGameType(ScheduledGameType.IntraconferenceFirstSet)
                    .Concat(diagnostic.GetOpponentsByGameType(ScheduledGameType.IntraconferenceSecondSet))
                    .ToArray();

                // Must be exactly four intraconference games/opponents
                if (intraconferenceOpponents.Length != 4)
                {
                    Assert.Fail($"Team {teamInfo.Name} has {intraconferenceOpponents.Length} intraconference games, expected 4 ({string.Join(", ", intraconferenceOpponents.Select(o => o.Name))}).");
                }

                // Check whether all intraconference opponents are from the team's own division
                var allFromOwnDivision = intraconferenceOpponents
                    .All(o => divisionOpponents.Any(d => d.Name == o.Name));

                if (allFromOwnDivision)
                {
                    // Each of the three division opponents should appear,
                    // with exactly one of them appearing twice (2 + 1 + 1 = 4)
                    var counts = divisionOpponents
                        .Select(d => intraconferenceOpponents.Count(o => o.Name == d.Name))
                        .OrderByDescending(c => c)
                        .ToArray();

                    // Ensure the multiset of counts equals {2,1,1}
                    Assert.Equal(3, counts.Length);
                    Assert.Equal(2, counts[0]);
                    Assert.Equal(1, counts[1]);
                    Assert.Equal(1, counts[2]);
                }
                else
                {
                    // Otherwise there should be four distinct opponents, each occurring exactly once
                    var distinctCount = intraconferenceOpponents.Select(o => o.Name).Distinct().Count();
                    Assert.Equal(4, distinctCount);

                    foreach (var name in intraconferenceOpponents.Select(o => o.Name).Distinct())
                    {
                        var occurrences = intraconferenceOpponents.Count(o => o.Name == name);
                        Assert.Equal(1, occurrences);
                    }
                }
            }
        }

        [Theory]
        [InlineData(2014)]
        [InlineData(2015)]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        public void InterconferenceOpponentsAreOutOfConference(int year)
        {
            var schedule = GenerateScheduleForYear(year);
            var context = new FootballContext();
            var teams = context.Teams.ToList();

            foreach (var teamEntity in teams)
            {
                var teamInfo = ToBasicTeamInfo(teamEntity);
                var diagnostic = schedule.Diagnostics.FirstOrDefault(d => d.Team!.Name == teamInfo.Name);
                Assert.NotNull(diagnostic);

                var interconferenceOpponents = diagnostic
                    .GetOpponentsByGameType(ScheduledGameType.Interconference);

                // There should be exactly four interconference games/opponents
                Assert.Equal(4, interconferenceOpponents.Count);

                // Each interconference opponent must be from the opposite conference
                foreach (var opponent in interconferenceOpponents)
                {
                    var opponentEntity = teams.FirstOrDefault(t => t.TeamName == opponent.Name);
                    Assert.NotNull(opponentEntity);
                    Assert.NotEqual(teamEntity.Conference, opponentEntity.Conference);
                }
            }
        }

        /*
         For each team in the DB:
           - Find the diagnostics entry for that team from the generated schedule.
           - Pull the "remaining" intraconference opponents from diagnostics. (These are the intraconference games beyond the first and second sets.)
             - We will request opponents with ScheduledGameType.IntraconferenceRemaining.
           - Ensure there are exactly two such opponents (the schedule rules state there are two "remaining" intraconference games).
           - Obtain the default previous-season division rankings using Helpers.GetDefaultPreviousSeasonDivisionRankings(year).
             - The helper likely returns a dictionary-like structure mapping a division key to an ordered list of team names (rank 1..4).
             - We will locate which division list contains the current team and determine the team's rank by index+1.
             - To keep the test robust against the exact return type, iterate the returned structure via IEnumerable and use reflection to access each entry's Value (the ranking list).
           - Determine whether the two remaining opponents are from the team's own division:
             - Use the teams list from the DB to find opponent entities and compare conference/division to the current team.
           - If opponents are NOT from the team's own division:
             - Both opponents (each from different out-of-division opponents) must have the same rank as the team (i.e., if team is rank R, each opponent must be rank R in their respective divisions).
           - If opponents ARE from the team's own division:
             - The schedule rule says there are two games and:
               - If the team rank is 1 => both opponents must be the rank-4 team (two games vs rank 4).
               - If the team rank is 2 => both opponents must be the rank-3 team (two games vs rank 3).
               - Otherwise this situation is unexpected and should fail the test.
         */

        [Theory]
        [InlineData(2014)]
        [InlineData(2015)]
        [InlineData(2016)]
        [InlineData(2017)]
        [InlineData(2018)]
        public void RemainingIntraconferenceGamesFollowDefaultRankings(int year)
        {
            var schedule = GenerateScheduleForYear(year);
            var context = new FootballContext();
            var teams = context.Teams.ToList();

            // Fetch default previous season rankings
            var defaultRankings = Helpers.GetDefaultPreviousSeasonDivisionRankings(teams.Select(ToBasicTeamInfo).ToArray());

            foreach (var teamEntity in teams)
            {
                var teamInfo = ToBasicTeamInfo(teamEntity);
                var diagnostic = schedule.Diagnostics.FirstOrDefault(d => d.Team!.Name == teamInfo.Name);
                Assert.NotNull(diagnostic);

                // Pull remaining intraconference opponents
                var remainingIntraconference = diagnostic
                    .GetOpponentsByGameType(ScheduledGameType.RemainingIntraconferenceFirstSet)
                    .Concat(diagnostic.GetOpponentsByGameType(ScheduledGameType.RemainingIntraconferenceSecondSet))
                    .ToArray();

                // There must be exactly two remaining intraconference games/opponents
                Assert.Equal(2, remainingIntraconference.Length);

                var opponentNames = remainingIntraconference.Select(o => o.Name).ToArray();

                // Find opponent entities from DB to examine their divisions
                var opponentEntities = opponentNames
                    .Select(name => teams.FirstOrDefault(t => t.TeamName == name))
                    .ToArray();

                Assert.All(opponentEntities, Assert.NotNull);

                // Get the team's rank in its division using the default rankings
                var teamRank = defaultRankings[teamInfo];
                Assert.InRange(teamRank, 1, 4);

                if (!diagnostic.TeamDivisionPlaysSelfForRemainingIntraconference)
                {
                    // Each opponent must be at the same rank as the team in their respective divisions
                    foreach (var opponent in opponentEntities)
                    {
                        var opponentRank = defaultRankings[ToBasicTeamInfo(opponent!)];
                        Assert.InRange(opponentRank, 1, 4);
                        Assert.Equal(teamRank, opponentRank);
                    }
                }
                else
                {
                    // Both opponents are from the same division as the team (team faces division again)
                    // Expect exactly two games (already asserted). Now validate rank pairing rules:
                    int expectedOpponentRank = teamRank switch
                    {
                        1 => 4,
                        2 => 3,
                        3 => 2,
                        4 => 1,
                        _ => throw new InvalidOperationException($"Unexpected scenario: Team {teamInfo.Name} has rank {teamRank} but is scheduled to face its own division in the remaining intraconference games."),
                    };

                    foreach (var opponent in opponentEntities)
                    {
                        var oppRank = defaultRankings[ToBasicTeamInfo(opponent!)];
                        Assert.Equal(expectedOpponentRank, oppRank);
                    }
                }
            }
        }

        private static BasicTeamInfo ToBasicTeamInfo(Team team)
        {
            return new BasicTeamInfo(team.TeamName, team.Conference, team.Division);
        }

        private static bool TeamPlaysInGame(BasicTeamInfo team, GameRecord game)
        {
            return game.HomeTeam!.TeamName == team.Name || game.AwayTeam!.TeamName == team.Name;
        }
    }
}
