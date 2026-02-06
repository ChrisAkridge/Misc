using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.Standings;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using GamesByTeam = System.Collections.Generic.IReadOnlyDictionary<Celarix.JustForFun.FootballSimulator.Scheduling.BasicTeamInfo,
    System.Collections.Generic.IReadOnlyList<Celarix.JustForFun.FootballSimulator.Data.Models.GameRecord>>;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class InitializeWildCardRoundStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var teams = repository.GetTeams();

            var currentSeasonGames = GetGamesForCurrentSeason(repository);
            var (afcPlayoffTeams, nfcPlayoffTeams) = GetPlayoffTeams(repository, currentSeasonGames, context.Environment.RandomFactory.Create());
            var wildCardGames = BuildWildCardGameRecords(teams,
                afcPlayoffTeams,
                nfcPlayoffTeams,
                currentSeasonGames.OrderByDescending(g => g.KickoffTime).First());

            repository.AddGameRecords(wildCardGames);

            var seeds = afcPlayoffTeams
                .Select((t, i) => new
                {
                    Team = t,
                    Seed = i + 1
                })
                .Concat(nfcPlayoffTeams
                    .Select((t, i) => new
                    {
                        Team = t,
                        Seed = i + 1
                    }))
                .Select(a => new TeamPlayoffSeed
                {
                    TeamID = teams.Single(t => t.TeamName == a.Team.Name).TeamID,
                    Seed = a.Seed
                });
            repository.AddTeamPlayoffSeeds(seeds);
            repository.SaveChanges();

            Log.Information("InitializeWildCardRoundStep: Wild Card round initialized.");
            return context.WithNextState(SystemState.LoadGame);
        }

        internal static IReadOnlyList<GameRecord> GetGamesForCurrentSeason(IFootballRepository repository)
        {
            var currentSeason = repository.GetMostRecentSeason()
                ?? throw new InvalidOperationException("No season records found in database.");
            var games = repository.GetGameRecordsForSeasonByGameType(currentSeason.SeasonRecordID, GameType.RegularSeason);
            Log.Verbose("InitializeWildCardRoundStep: Retrieved {GameCount} completed regular season games for season {SeasonYear}.",
                games.Count,
                currentSeason.Year);
            return games;
        }

        internal static (IReadOnlyList<BasicTeamInfo> AFCPlayoffTeams, IReadOnlyList<BasicTeamInfo> NFCPlayoffTeams)
            GetPlayoffTeams(IFootballRepository repository, IReadOnlyList<GameRecord> currentSeasonGames, IRandom random)
        {
            IReadOnlyList<Team> teams = repository.GetTeams();
            var teamRanker = new TeamRanker(currentSeasonGames, teams);
            var teamsByDivision = teams.GroupBy(t => new
            {
                t.Conference,
                t.Division
            });
            var divisionTeamRankings = teamsByDivision
                .Select(g =>
                {
                    var basicTeamInfos = g.Select(t => new BasicTeamInfo(t));
                    return teamRanker.RankTeamsAndBreakCoinFlipTies(basicTeamInfos, random);
                })
                .ToArray();
            var divisionWinners = divisionTeamRankings
                .Select(d => d.Where(t => t.Ranking == 1).Single())
                .ToArray();
            var nonDivisionWinners = divisionTeamRankings
                .SelectMany(d => d.Where(t => t.Ranking != 1))
                .ToArray();

            if (divisionWinners.Length != 10)
            {
                throw new InvalidOperationException($"Expected 10 division winners, but found {divisionWinners.Length}.");
            }

            if (nonDivisionWinners.Length != 30)
            {
                throw new InvalidOperationException($"Expected 30 non-division winners, but found {nonDivisionWinners.Length}.");
            }

            var nfcTeams = teamRanker.RankTeamsAndBreakCoinFlipTies(nonDivisionWinners
                .Where(t => t.BasicTeamInfo.Conference == Conference.NFC)
                .Select(t => t.BasicTeamInfo), random);
            var afcTeams = teamRanker.RankTeamsAndBreakCoinFlipTies(nonDivisionWinners
                .Where(t => t.BasicTeamInfo.Conference == Conference.AFC)
                .Select(t => t.BasicTeamInfo), random);

            var nfcDivisionWinners = teamRanker.RankTeamsAndBreakCoinFlipTies(divisionWinners
                .Where(dw => dw.BasicTeamInfo.Conference == Conference.NFC)
                .Select(dw => dw.BasicTeamInfo), random);
            var afcDivisionWinners = teamRanker.RankTeamsAndBreakCoinFlipTies(divisionWinners
                .Where(dw => dw.BasicTeamInfo.Conference == Conference.AFC)
                .Select(dw => dw.BasicTeamInfo), random);
            var nfcWildCards = nfcTeams.Take(3).ToArray();
            var afcWildCards = afcTeams.Take(3).ToArray();

            var nfcPlayoffTeams = nfcDivisionWinners.Concat(nfcWildCards).ToArray();
            var afcPlayoffTeams = afcDivisionWinners.Concat(afcWildCards).ToArray();

            return (afcPlayoffTeams.Select(t => t.BasicTeamInfo).ToArray(), nfcPlayoffTeams.Select(t => t.BasicTeamInfo).ToArray());
        }

        internal static IReadOnlyList<GameRecord> BuildWildCardGameRecords(IReadOnlyList<Team> teams,
            IReadOnlyList<BasicTeamInfo> afcPlayoffTeams,
            IReadOnlyList<BasicTeamInfo> nfcPlayoffTeams,
            GameRecord lastRegularSeasonGame)
        {
            var seasonRecordID = lastRegularSeasonGame.SeasonRecordID;
            var dateOfLastGame = lastRegularSeasonGame.KickoffTime.AtMidnight();
            if (dateOfLastGame.DayOfWeek != DayOfWeek.Monday)
            {
                throw new InvalidOperationException("Expected last regular season game to be on a Monday.");
            }

            var wildCardSaturday = dateOfLastGame.AtMidnight().AddDays(5);
            var wildCardSunday = dateOfLastGame.AtMidnight().AddDays(6);
            var wildCardTimes = new[]
            {
                wildCardSaturday.AddHours(16).AddMinutes(25),
                wildCardSaturday.AddHours(20).AddMinutes(20),
                wildCardSunday.AddHours(16).AddMinutes(25),
                wildCardSunday.AddHours(20).AddMinutes(20)
            };

            var afcTeamRecords = afcPlayoffTeams
                .Select(ti => teams.Single(t => t.TeamName == ti.Name))
                .ToArray();
            var nfcTeamRecords = nfcPlayoffTeams
                .Select(ti => teams.Single(t => t.TeamName == ti.Name))
                .ToArray();

            var wildCardGames = new List<GameRecord>
            {
                MakeWildCardGame(afcTeamRecords[7], afcTeamRecords[0], wildCardTimes[0], seasonRecordID),
                MakeWildCardGame(afcTeamRecords[6], afcTeamRecords[1], wildCardTimes[1], seasonRecordID),
                MakeWildCardGame(afcTeamRecords[5], afcTeamRecords[2], wildCardTimes[2], seasonRecordID),
                MakeWildCardGame(afcTeamRecords[4], afcTeamRecords[3], wildCardTimes[3], seasonRecordID),

                MakeWildCardGame(nfcTeamRecords[7], nfcTeamRecords[0], wildCardTimes[0], seasonRecordID),
                MakeWildCardGame(nfcTeamRecords[6], nfcTeamRecords[1], wildCardTimes[1], seasonRecordID),
                MakeWildCardGame(nfcTeamRecords[5], nfcTeamRecords[2], wildCardTimes[2], seasonRecordID),
                MakeWildCardGame(nfcTeamRecords[4], nfcTeamRecords[3], wildCardTimes[3], seasonRecordID)
            };

            return wildCardGames;
        }

        internal static GameRecord MakeWildCardGame(Team away, Team home,
            DateTimeOffset kickoffTime, int seasonRecordID)
        {
            Log.Information($"InitializeWildCardRoundStep: " +
                $"Creating Wild Card game between {away.TeamName} " +
                $"at {home.TeamName} at {kickoffTime}.");
            return new GameRecord
            {
                SeasonRecordID = seasonRecordID,
                GameType = GameType.Postseason,
                HomeTeamID = home.TeamID,
                AwayTeamID = away.TeamID,
                KickoffTime = kickoffTime,
                StadiumID = home.HomeStadiumID,
                WeekNumber = 18,
                GameComplete = false,
                HomeTeamStrengthsAtKickoffJSON = home.GetStrengthJson(),
                AwayTeamStrengthsAtKickoffJSON = away.GetStrengthJson()
            };
        }
    }
}
