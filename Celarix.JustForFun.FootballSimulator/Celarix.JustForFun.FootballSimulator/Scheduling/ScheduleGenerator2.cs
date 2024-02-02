using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

public static class ScheduleGenerator2
{
    // WYLO: aaaaaaaaaaa
    // man, writing this all in English makes it seem so easy
    // we need to break down every problem the schedule generator has to solve
    // and we need better relationships between simple, comparable team representations
    // and their full database object counterparts. The schedule generator is so intrinsically
    // tied to the idea of exactly 40 teams across 10 divisions and 2 conferences, though. I
    // really don't want to make it seem like it supports any other configuration.
    
    // Okay, how about this:
    // - Use BasicTeamInfo instead of strings to represent teams
    // - Take in a list of Team DB objects, check if it has exactly 40 teams, and throw if not
    // - Convert those teams into BasicTeamInfo and use that for the rest of the schedule generation
    
    private const int YearZero = 2014;
    
    private static SymmetricTable<BasicTeamInfo?> GetTeamOpponentsForSeason(BasicTeamInfo[] teams,
        int seasonYear,
        Dictionary<BasicTeamInfo, int>? previousSeasonDivisionRankings)
    {
        var yearCycleNumber = (seasonYear - YearZero) % 5;
        var opponents = SymmetricTable<BasicTeamInfo>.FromRowKeys(teams, 16, new BasicTeamInfoComparer());
        previousSeasonDivisionRankings ??= GetDefaultPreviousSeasonDivisionRankings(teams);

        foreach (var team in teams)
        {
            var teamConference = team.Conference;
            var teamDivision = team.Division;
            
            var intradivisionOpponents = GetTeamsInDivision(teamConference, teamDivision).Where(t => team != t).ToArray();
            var intraconferenceOpponents = GetTeamsInDivision(teamConference, GetIntraconferenceOpponentDivisions(yearCycleNumber, teamDivision));
            var interconferenceOpponents = GetTeamsInDivision(teamConference == Conference.AFC
                ? Conference.NFC
                : Conference.AFC, GetInterconferenceOpponentDivision(yearCycleNumber, teamConference, teamDivision));
            var remainingIntraconferenceOpponents = GetRemainingIntraconferenceOpponentTeams(teamConference,
                teamDivision,
                team,
                yearCycleNumber,
                previousSeasonDivisionRankings);

            for (int i = 0; i < 16; i++)
            {
                if (opponents[team, i] != null) { continue; }
                
                opponents[team, i] = i switch
                {
                    < 6 => intradivisionOpponents[i / 2],
                    < 10 => intraconferenceOpponents[i - 6],
                    < 14 => interconferenceOpponents[i - 10],
                    _ => remainingIntraconferenceOpponents[i - 14]
                };
            }
        }
        
        return opponents;
    }

    private static List<GameRecord> GetGameRecordsFromOpponentTable(SymmetricTable<string?> opponents,
        IReadOnlyList<Team> dataTeams)
    {
        var gameRecords = new List<GameRecord>(320);

        foreach (var team in opponents.Keys)
        {
            for (int i = 0; i < 16; i++)
            {
                if (opponents[team, i] == null) { continue; }
                var keyTeamIsHome = i % 2 == 0;
                var homeTeam = keyTeamIsHome ? team : opponents[team, i];
                var awayTeam = keyTeamIsHome ? opponents[team, i] : team;
                
                gameRecords.Add(new GameRecord
                {
                    GameType = GameType.RegularSeason,
                    WeekNumber = 0,
                    GameComplete = false,
                    HomeTeam = dataTeams.First(t => t == homeTeam),
                    AwayTeam = dataTeams.First(t => t.TeamName == awayTeam),
                    StadiumID = 0,
                    Stadium = null,
                    KickoffTime = default,
                    TemperatureAtKickoff = 0,
                    WeatherAtKickoff = StadiumWeather.Sunny,
                    HomeTeamStrengthsAtKickoffJSON = null,
                    AwayTeamStrengthsAtKickoffJSON = null,
                    QuarterBoxScores = null,
                    TeamGameRecords = null,
                    TeamDriveRecords = null
                });
            }
        }
    }

    private static string[] GetRemainingIntraconferenceOpponentTeams(Conference conference,
        Division division,
        string team,
        int yearCycleNumber,
        IReadOnlyDictionary<string, int> previousSeasonDivisionRankings)

    {
        var opponentDivisions = GetRemainingIntraconferenceOpponentDivisions(yearCycleNumber, division);

        if (opponentDivisions.Item1 == opponentDivisions.Item2)
        {
            var divisionTeams = GetTeamsInDivision(conference, opponentDivisions.Item1);
            var teamsByRanking = previousSeasonDivisionRankings.Where(kvp => divisionTeams.Contains(kvp.Key))
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();
            
            var teamRanking = previousSeasonDivisionRankings[team];

            return teamRanking switch
            {
                1 => Enumerable.Repeat(teamsByRanking[3], 2).ToArray(),
                2 => Enumerable.Repeat(teamsByRanking[2], 2).ToArray(),
                3 => Enumerable.Repeat(teamsByRanking[1], 2).ToArray(),
                4 => Enumerable.Repeat(teamsByRanking[0], 2).ToArray(),
                _ => throw new ArgumentOutOfRangeException(nameof(teamRanking))
            };
        }
        else
        {
            var opponentDivisionTeams1 = GetTeamsInDivision(conference, opponentDivisions.Item1);
            var opponentDivisionTeams2 = GetTeamsInDivision(conference, opponentDivisions.Item2);

            var opponents1ByRanking = previousSeasonDivisionRankings
                .Where(kvp => opponentDivisionTeams1.Contains(kvp.Key))
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();

            var opponents2ByRanking = previousSeasonDivisionRankings
                .Where(kvp => opponentDivisionTeams2.Contains(kvp.Key))
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToArray();

            var teamRanking = previousSeasonDivisionRankings[team];

            return new[]
            {
                opponents1ByRanking[teamRanking - 1], opponents2ByRanking[teamRanking - 1]
            };
        }
    }
    
    private static Conference GetConferenceForTeam(string teamName) =>
        teamNames.IndexOf(teamName) < 20
            ? Conference.AFC
            : Conference.NFC;
    
    private static Division GetDivisionForTeam(string teamName) =>
        (teamNames.IndexOf(teamName) % 20) switch
        {
            < 4 => Division.North,
            < 8 => Division.South,
            < 12 => Division.East,
            < 16 => Division.West,
            < 20 => Division.Extra,
            _ => throw new ArgumentOutOfRangeException(nameof(teamName))
        };

    private static string[] GetTeamsInDivision(Conference conference, Division division)
    {
        var indexBasis = conference == Conference.AFC ? 0 : 20;

        indexBasis += division switch
        {
            Division.North => 0,
            Division.South => 4,
            Division.East => 8,
            Division.West => 12,
            Division.Extra => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(division))
        };
        
        return teamNames[indexBasis..(indexBasis + 4)];
    }

    private static Dictionary<BasicTeamInfo, int> GetDefaultPreviousSeasonDivisionRankings(BasicTeamInfo[] teams)
    {
        var random = new Random(-1528635010);
        var rankings = new Dictionary<BasicTeamInfo, int>();

        for (int i = 0; i < teams.Length; i += 4)
        {
            var divisionTeams = teams[i..(i + 4)];
            divisionTeams.Shuffle(random);

            for (var j = 0; j < divisionTeams.Length; j++)
            {
                var team = divisionTeams[j];
                rankings.Add(team, j);
            }
        }

        return rankings;
    }
}