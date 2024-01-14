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
    
    private const int YearZero = 2014;
    private static readonly string[] teamNames =
    {
        "Cincinnati", "Baltimore", "Pittsburgh", "Cleveland",
        "Indianapolis", "Tennessee", "Houston", "Jacksonville",
        "Buffalo", "New England", "Miami", "New York Jets",
        "Denver", "Oakland", "San Diego", "Kansas City",
        "Louisville", "Toledo", "Portales", "Vostok Station",
        
        "Detroit", "Chicago", "Green Bay", "Minnesota", 
        "Atlanta", "Carolina", "Tampa Bay", "New Orleans",
        "New York Giants", "Philadelphia", "Washington", "Dallas",
        "Seattle", "San Francisco", "St. Louis", "Arizona",
        "Furnace Creek", "Dover", "Grand Forks", "Wainwright"
    };
    
    private static SymmetricTable<string?> GetTeamOpponentsForSeason(int seasonYear,
        Dictionary<string, int>? previousSeasonDivisionRankings)
    {
        var yearCycleNumber = (seasonYear - YearZero) % 5;
        var opponents = SymmetricTable<string>.FromRowKeys(teamNames, 16, StringComparer.OrdinalIgnoreCase);
        previousSeasonDivisionRankings ??= GetDefaultPreviousSeasonDivisionRankings();

        foreach (var team in teamNames)
        {
            var teamConference = GetConferenceForTeam(team);
            var teamDivision = GetDivisionForTeam(team);
            
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
    
    private static Division GetIntraconferenceOpponentDivisions(int yearCycleNumber, Division division)
    {
        const Division N = Division.North;
        const Division S = Division.South;
        const Division E = Division.East;
        const Division W = Division.West;
        const Division X = Division.Extra;
        
        return yearCycleNumber switch
        {
            0 => division switch
            {
                E => W,
                W => E,
                N => S,
                S => N,
                X => X,
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            1 => division switch
            {
                E => S,
                S => E,
                N => X,
                X => N,
                W => W,
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            2 => division switch
            {
                E => N,
                N => E,
                W => X,
                X => W,
                S => S,
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            3 => division switch
            {
                E => X,
                X => E,
                W => S,
                S => W,
                N => N,
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            4 => division switch
            {
                X => N,
                N => X,
                W => S,
                S => W,
                E => E,
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
        };
    }

    private static Division GetInterconferenceOpponentDivision(int yearCycleNumber, Conference conference,
        Division division)
    {
        const Conference AFC = Conference.AFC;
        const Conference NFC = Conference.NFC;

        const Division N = Division.North;
        const Division S = Division.South;
        const Division E = Division.East;
        const Division W = Division.West;
        const Division X = Division.Extra;

        return conference switch
        {
            AFC => division switch
            {
                E => yearCycleNumber switch
                {
                    0 => N,
                    1 => E,
                    2 => W,
                    3 => S,
                    4 => X,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                N => yearCycleNumber switch
                {
                    0 => S,
                    1 => W,
                    2 => E,
                    3 => X,
                    4 => N,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                S => yearCycleNumber switch
                {
                    0 => E,
                    1 => S,
                    2 => X,
                    3 => N,
                    4 => W,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                W => yearCycleNumber switch
                {
                    0 => W,
                    1 => X,
                    2 => N,
                    3 => E,
                    4 => S,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                X => yearCycleNumber switch
                {
                    0 => X,
                    1 => N,
                    2 => S,
                    3 => W,
                    4 => E,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            NFC => division switch
            {
                E => yearCycleNumber switch
                {
                    0 => S,
                    1 => E,
                    2 => N,
                    3 => W,
                    4 => X,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                N => yearCycleNumber switch
                {
                    0 => E,
                    1 => X,
                    2 => W,
                    3 => S,
                    4 => N,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                S => yearCycleNumber switch
                {
                    0 => N,
                    1 => S,
                    2 => X,
                    3 => E,
                    4 => W,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                W => yearCycleNumber switch
                {
                    0 => W,
                    1 => N,
                    2 => E,
                    3 => X,
                    4 => S,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                X => yearCycleNumber switch
                {
                    0 => X,
                    1 => W,
                    2 => S,
                    3 => N,
                    4 => E,
                    _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                },
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            _ => throw new ArgumentOutOfRangeException(nameof(conference))
        };
    }

    private static (Division, Division) GetRemainingIntraconferenceOpponentDivisions(int yearCycleNumber,
        Division division)
    {
        const Division N = Division.North;
        const Division S = Division.South;
        const Division E = Division.East;
        const Division W = Division.West;
        const Division X = Division.Extra;

        return yearCycleNumber switch
        {
            0 => division switch
            {
                N => (W, E),
                S => (S, S),
                E => (X, N),
                W => (N, X),
                X => (E, W),
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            1 => division switch
            {
                N => (E, S),
                S => (W, N),
                E => (N, W),
                W => (S, E),
                X => (X, X),
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            2 => division switch
            {
                N => (W, X),
                S => (X, W),
                E => (E, E),
                W => (N, S),
                X => (S, N),
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            3 => division switch
            {
                N => (S, E),
                S => (N, X),
                E => (X, N),
                W => (W, W),
                X => (E, S),
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            4 => division switch
            {
                N => (N, N),
                S => (E, W),
                E => (S, X),
                W => (X, S),
                X => (W, E),
                _ => throw new ArgumentOutOfRangeException(nameof(division))
            },
            _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
        };
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

    private static Dictionary<string, int> GetDefaultPreviousSeasonDivisionRankings()
    {
        var random = new Random(-1528635010);
        var rankings = new Dictionary<string, int>();

        for (int i = 0; i < teamNames.Length; i += 4)
        {
            var divisionTeams = teamNames[i..(i + 4)];
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