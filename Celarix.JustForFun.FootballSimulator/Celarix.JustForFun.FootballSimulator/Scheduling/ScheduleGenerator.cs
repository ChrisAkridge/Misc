using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

/*internal*/
public static class ScheduleGenerator
{
    public static List<GameRecord> GetPreseasonAndRegularSeasonGamesForSeason(int seasonYear,
        List<Team> teams,
        Dictionary<string, int>? previousSeasonTeamPositions)
    {
        // Bleh. 2015 season year fails. Wipe out database, run migrations, start again.
        // Or maybe just comment out the part that makes the season record.
        var basicTeamInfos = teams
            .Select(t => new BasicTeamInfo
            {
                Name = t.TeamName,
                Conference = t.Conference,
                Division = t.Division
            })
            .ToList();
        var regularSeasonMatchups =
            GetAllRegularSeasonMatchupsForSeasonYear(basicTeamInfos, seasonYear, previousSeasonTeamPositions);
        var regularSeasonWeeks = GetRegularSeasonTimeslotsForGames(seasonYear, regularSeasonMatchups);
        var preseasonWeeks = GetAllPreseasonMatchupsForSeasonYear(basicTeamInfos, GetAllMatchupsForSeason(regularSeasonMatchups),
            GetNthWeekdayOfMonth(seasonYear, 9, DayOfWeek.Thursday, 2));
        
        return preseasonWeeks.Select((pw, i) => ConvertWeekToGameRecords(pw, teams, i + 1, false))
            .Concat(regularSeasonWeeks.Select((rw, i) => ConvertWeekToGameRecords(rw, teams, i + 1, true)))
            .SelectMany(week => week)
            .OrderBy(gr => gr.KickoffTime)
            .ToList();
    }

    private static IEnumerable<GameRecord> ConvertWeekToGameRecords(IEnumerable<(DateTimeOffset gameTime, GameMatchup game)> week,
        IReadOnlyCollection<Team> teams,
        int weekNumber,
        bool isRegularSeasonWeek) =>
        week.Select(timeAndGame => new GameRecord
        {
            GameType = isRegularSeasonWeek ? GameType.RegularSeason : GameType.Preseason,
            WeekNumber = weekNumber,
            HomeTeam = teams.First(t => t.TeamName == timeAndGame.game.HomeTeam.Name),
            AwayTeam = teams.First(t => t.TeamName == timeAndGame.game.AwayTeam.Name),
            Stadium = teams.First(t => t.TeamName == timeAndGame.game.HomeTeam.Name).HomeStadium,
            KickoffTime = timeAndGame.gameTime
        });

    #region Matchup Generation
    private static Dictionary<(Conference conference, Division division), DivisionMatchupsForSeason[]> GetDivisionMatchupCycle()
    {
        const Division N = Division.North;
        const Division S = Division.South;
        const Division E = Division.East;
        const Division W = Division.West;
        const Division X = Division.Extra;

        return new Dictionary<(Conference conference, Division division), DivisionMatchupsForSeason[]>
        {
            {
                (Conference.AFC, N), new[]
                {
                    new DivisionMatchupsForSeason(S, S, X, W),
                    new DivisionMatchupsForSeason(X, W, S, E),
                    new DivisionMatchupsForSeason(E, E, W, S),
                    new DivisionMatchupsForSeason(N, X, E, W),
                    new DivisionMatchupsForSeason(X, N, E, X)
                }
            },
            {
                (Conference.AFC, S), new[]
                {
                    new DivisionMatchupsForSeason(N, E, W, E),
                    new DivisionMatchupsForSeason(E, S, N, W),
                    new DivisionMatchupsForSeason(S, X, N, E),
                    new DivisionMatchupsForSeason(W, N, E, X),
                    new DivisionMatchupsForSeason(W, W, E, W)
                }
            },
            {
                (Conference.AFC, E), new[]
                {
                    new DivisionMatchupsForSeason(W, N, S, X),
                    new DivisionMatchupsForSeason(S, E, N, X),
                    new DivisionMatchupsForSeason(N, W, S, X),
                    new DivisionMatchupsForSeason(X, S, N, S),
                    new DivisionMatchupsForSeason(E, X, N, W)
                }
            },
            {
                (Conference.AFC, W), new[]
                {
                    new DivisionMatchupsForSeason(E, W, N, S),
                    new DivisionMatchupsForSeason(W, X, S, X),
                    new DivisionMatchupsForSeason(X, N, N, X),
                    new DivisionMatchupsForSeason(S, E, N, X),
                    new DivisionMatchupsForSeason(S, S, X, S)
                }
            },
            {
                (Conference.AFC, X), new[]
                {
                    new DivisionMatchupsForSeason(X, X, N, E),
                    new DivisionMatchupsForSeason(N, N, E, W),
                    new DivisionMatchupsForSeason(W, S, E, W),
                    new DivisionMatchupsForSeason(E, W, S, W),
                    new DivisionMatchupsForSeason(N, E, N, S)
                }
            },
            {
                (Conference.NFC, N), new[]
                {
                    new DivisionMatchupsForSeason(S, E, X, W),
                    new DivisionMatchupsForSeason(X, X, S, E),
                    new DivisionMatchupsForSeason(E, W, W, S),
                    new DivisionMatchupsForSeason(N, S, E, W),
                    new DivisionMatchupsForSeason(X, N, E, X)
                }
            },
            {
                (Conference.NFC, S), new[]
                {
                    new DivisionMatchupsForSeason(N, N, W, E),
                    new DivisionMatchupsForSeason(E, S, N, W),
                    new DivisionMatchupsForSeason(S, X, N, E),
                    new DivisionMatchupsForSeason(W, E, E, X),
                    new DivisionMatchupsForSeason(W, W, W, X)
                }
            },
            {
                (Conference.NFC, E), new[]
                {
                    new DivisionMatchupsForSeason(W, S, S, X),
                    new DivisionMatchupsForSeason(S, E, N, X),
                    new DivisionMatchupsForSeason(N, N, S, X),
                    new DivisionMatchupsForSeason(X, W, N, S),
                    new DivisionMatchupsForSeason(E, X, N, S)
                }
            },
            {
                (Conference.NFC, W), new[]
                {
                    new DivisionMatchupsForSeason(E, W, N, S),
                    new DivisionMatchupsForSeason(W, N, S, X),
                    new DivisionMatchupsForSeason(X, E, N, X),
                    new DivisionMatchupsForSeason(S, X, N, X),
                    new DivisionMatchupsForSeason(S, S, S, E)
                }
            },
            {
                (Conference.NFC, X), new[]
                {
                    new DivisionMatchupsForSeason(X, X, N, E),
                    new DivisionMatchupsForSeason(N, W, E, W),
                    new DivisionMatchupsForSeason(W, S, E, W),
                    new DivisionMatchupsForSeason(E, N, S, W),
                    new DivisionMatchupsForSeason(N, E, N, W)
                }
            },
        };
    }

    private static Dictionary<BasicTeamInfo, List<GameMatchup>> GetAllRegularSeasonMatchupsForSeasonYear(List<BasicTeamInfo> basicTeamInfos,
        int seasonYear,
        IReadOnlyDictionary<string, int>? previousSeasonTeamPositions)
    {
        var regularSeasonMatchups = basicTeamInfos.ToDictionary(i => i, _ => new List<GameMatchup>(16));
        var divisionMatchupCycle = GetDivisionMatchupCycle();
        var cycleYear = (seasonYear - 2014) % 5;

        foreach (var team in basicTeamInfos)
        {
            // Type I games: teams in this division (6 games)
            var otherTeamsInDivision = basicTeamInfos
                .Where(i => i.Conference == team.Conference
                    && i.Division == team.Division
                    && i.Name != team.Name);

            foreach (var otherDivisionTeam in otherTeamsInDivision)
            {
                AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                {
                    AwayTeam = otherDivisionTeam,
                    HomeTeam = team,
                    GameType = 1,
                    AddedBy = team,
                    GamePairId = Guid.NewGuid()
                });

                AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                {
                    AwayTeam = team,
                    HomeTeam = otherDivisionTeam,
                    GameType = 1,
                    AddedBy = team,
                    GamePairId = Guid.NewGuid()
                });
            }

            var typeIIOpponentDivision = divisionMatchupCycle[(team.Conference, team.Division)][cycleYear]
                .TypeIIOpponentDivision;
            // Type II games: intraconference games (4 games)
            var allTypeIIOpponentTeams =
                basicTeamInfos
                    .Where(t => t.Conference == team.Conference
                        && t.Division == typeIIOpponentDivision)
                    .ToList();
            var orderedTypeIIOpponentTeams = allTypeIIOpponentTeams.ToArray();

            if (typeIIOpponentDivision == team.Division)
            {
                allTypeIIOpponentTeams.Sort();

                int teamIndexInDivision = allTypeIIOpponentTeams.IndexOf(team);

                orderedTypeIIOpponentTeams = teamIndexInDivision switch
                {
                    0 => new[]
                    {
                        allTypeIIOpponentTeams[1], allTypeIIOpponentTeams[1], allTypeIIOpponentTeams[2],
                        allTypeIIOpponentTeams[3]
                    },
                    1 => new[]
                    {
                        allTypeIIOpponentTeams[0], allTypeIIOpponentTeams[0], allTypeIIOpponentTeams[3],
                        allTypeIIOpponentTeams[2]
                    },
                    2 => new[]
                    {
                        allTypeIIOpponentTeams[3], allTypeIIOpponentTeams[3], allTypeIIOpponentTeams[0],
                        allTypeIIOpponentTeams[1]
                    },
                    3 => new[]
                    {
                        allTypeIIOpponentTeams[2], allTypeIIOpponentTeams[2], allTypeIIOpponentTeams[1],
                        allTypeIIOpponentTeams[0]
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
                
            // do you ever get the feeling that this should be way easier?
            for (int i = 0; i < 4; i++)
            {
                if (i < 2)
                {
                    // When a division faces itself for type II games, the first 2 games are always
                    // against the same team.
                    AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                    {
                        AwayTeam = orderedTypeIIOpponentTeams![i],
                        HomeTeam = team,
                        GameType = 2,
                        AddedBy = team,
                        GamePairId = Guid.NewGuid()
                    });
                }
                else
                {
                    var secondHalfGame = new GameMatchup
                    {
                        AwayTeam = team,
                        HomeTeam = orderedTypeIIOpponentTeams![i],
                        GameType = 2,
                        AddedBy = team,
                        GamePairId = Guid.NewGuid()
                    };

                    if (!regularSeasonMatchups[team].Any(g => g.SymmetricallyEquals(secondHalfGame)))
                    {
                        AssignGameToBothTeams(regularSeasonMatchups, secondHalfGame);
                    }
                }
            }

            // Type III games: interconference games (4 games)
            var typeIIIOpponentDivision =
                divisionMatchupCycle[(team.Conference, team.Division)][cycleYear].TypeIIIOpponentDivision;

            var typeIIIOpponentTeams = basicTeamInfos.Where(t => t.Conference
                    == team.Conference switch
                    {
                        Conference.AFC => Conference.NFC,
                        Conference.NFC => Conference.AFC,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                    && t.Division == typeIIIOpponentDivision)
                .ToList();

            for (int i = 0; i < 4; i++)
            {
                AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                {
                    AwayTeam = i < 2 ? typeIIIOpponentTeams[i] : team,
                    HomeTeam = i < 2 ? team : typeIIIOpponentTeams[i],
                    GameType = 3,
                    AddedBy = team,
                    GamePairId = Guid.NewGuid()
                });
            }

            // Type IV games: other divisions based on standings
            var typeIVOpponentDivisions = divisionMatchupCycle[(team.Conference, team.Division)][cycleYear]
                .TypeIVOpponentDivisions;
            var typeIVOpponentTeams = new BasicTeamInfo[2];

            if (previousSeasonTeamPositions == null)
            {
                var teamDivisionNames = basicTeamInfos
                    .Where(t => t.Conference == team.Conference && t.Division == team.Division)
                    .ToList();
                teamDivisionNames.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                var teamSortedIndex = teamDivisionNames.IndexOf(team);
                    
                // No previous season info, choose two teams at random.
                typeIVOpponentTeams[0] = basicTeamInfos
                    .Where(t => t.Conference != team.Conference && t.Division == typeIVOpponentDivisions[0])
                    .OrderBy(t => t.Name)
                    .ElementAt(teamSortedIndex);

                typeIVOpponentTeams[1] = basicTeamInfos
                    .Where(t => t.Conference != team.Conference && t.Division == typeIVOpponentDivisions[1])
                    .OrderBy(t => t.Name)
                    .ElementAt(teamSortedIndex);
            }
            else
            {
                typeIVOpponentTeams[0] = basicTeamInfos
                    .Single(t => t.Conference != team.Conference
                        && t.Division == typeIVOpponentDivisions[0]
                        && previousSeasonTeamPositions[t.Name] == previousSeasonTeamPositions[team.Name]);

                typeIVOpponentTeams[1] = basicTeamInfos
                    .Single(t => t.Conference != team.Conference
                        && t.Division == typeIVOpponentDivisions[1]
                        && previousSeasonTeamPositions[t.Name] == previousSeasonTeamPositions[team.Name]);
            }

            AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
            {
                AwayTeam = typeIVOpponentTeams[0],
                HomeTeam = team,
                GameType = 4,
                AddedBy = team,
                GamePairId = Guid.NewGuid()
            });

            AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
            {
                AwayTeam = team,
                HomeTeam = typeIVOpponentTeams[1],
                GameType = 4,
                AddedBy = team,
                GamePairId = Guid.NewGuid()
            });
        }
            
        return regularSeasonMatchups;
    }

    private static void AssignGameToBothTeams(IReadOnlyDictionary<BasicTeamInfo, List<GameMatchup>> games,
        GameMatchup game)
    {   
        if (GameIsNotAlreadyPresent(games[game.AwayTeam], game)) { games[game.AwayTeam].Add(game); }

        if (GameIsNotAlreadyPresent(games[game.HomeTeam], game)) { games[game.HomeTeam].Add(game); }
    }

    private static bool GameIsNotAlreadyPresent(IReadOnlyCollection<GameMatchup> teamGames, GameMatchup gameToAdd)
    {
        var noOtherGameIsSymmetricallyEqual = teamGames.All(g => !g.SymmetricallyEquals(gameToAdd));

        var onlyOneTypeIMatchupBetweenTeams =
            gameToAdd.GameType == 1 && teamGames.Count(g => g.SymmetricallyEquals(gameToAdd)) == 1;
        var roomForMoreTypeIIGames = gameToAdd.GameType == 2 && teamGames.Count(g => g.GameType == 2) < 4;

        var onlyOneTypeIIMatchupBetweenTeams =
            gameToAdd.GameType == 2 && teamGames.Count(g => g.SymmetricallyEquals(gameToAdd)) == 1;

        return noOtherGameIsSymmetricallyEqual
            || onlyOneTypeIMatchupBetweenTeams
            || (roomForMoreTypeIIGames && onlyOneTypeIIMatchupBetweenTeams);
    }
    #endregion

    #region Scheduling Games
    private static IEnumerable<List<(DateTimeOffset gameTime, GameMatchup game)>> GetRegularSeasonTimeslotsForGames(int seasonYear,
        Dictionary<BasicTeamInfo, List<GameMatchup>> regularSeasonMatchups)
    {
        var regularSeasonWeekStartDates = GetRegularSeasonWeekStartDatesForYear(seasonYear);
        var gamesByWeek = MoveGamesForByes(SeparateGamesIntoWeeks(DeduplicateMatchups(regularSeasonMatchups)));
        var regularSeasonWeeks = new List<(DateTimeOffset gameTime, GameMatchup game)>[17];

        for (var weekNumber = 0; weekNumber < gamesByWeek.Length; weekNumber++)
        {
            regularSeasonWeeks[weekNumber] = new List<(DateTimeOffset gameTime, GameMatchup game)>();
                
            var gamesInWeekStack = new Stack<GameMatchup>(gamesByWeek[weekNumber]);
            var weekStartDate = regularSeasonWeekStartDates[weekNumber];

            // Fill in the primetime games.
            var eightThirtyPM = new TimeSpan(20, 30, 0);

            var primetimeGameTimes = new[]
            {
                weekStartDate.Add(eightThirtyPM), weekStartDate.AddDays(3).Add(eightThirtyPM),
                weekStartDate.AddDays(4).Add(eightThirtyPM)
            };

            foreach (var primetimeGameTime in primetimeGameTimes)
            {
                regularSeasonWeeks[weekNumber].Add((primetimeGameTime, gamesInWeekStack.Pop()));
            }

            // Fill in the 2 4:30pm games.
            var sundayAtFourThirtyPM = weekStartDate.AddDays(3).Add(new TimeSpan(16, 30, 0));
            regularSeasonWeeks[weekNumber].Add((sundayAtFourThirtyPM, gamesInWeekStack.Pop()));
            regularSeasonWeeks[weekNumber].Add((sundayAtFourThirtyPM, gamesInWeekStack.Pop()));
                
            // Fill in the rest of the 1:00pm games.
            var sundayAtOnePM = weekStartDate.AddDays(3).Add(new TimeSpan(13, 0, 0));

            while (gamesInWeekStack.Any())
            {
                regularSeasonWeeks[weekNumber].Add((sundayAtOnePM, gamesInWeekStack.Pop()));
            }
        }
        return regularSeasonWeeks;
    }

    private static IEnumerable<GameMatchup> DeduplicateMatchups(Dictionary<BasicTeamInfo, List<GameMatchup>> regularSeasonMatchups)
    {
        var hashSet = new HashSet<GameMatchup>(new GameMatchupPairComparer());
        var debugSeenGames = 0;
            
        foreach (var teamGameMatchup in regularSeasonMatchups.SelectMany(kvp => kvp.Value))
        {
            debugSeenGames += 1;
            var gameAdded = hashSet.Add(teamGameMatchup);
                
            Console.WriteLine(gameAdded
                ? $"#{debugSeenGames}: Game {teamGameMatchup} added to list."
                : $"#{debugSeenGames}: Duplicate. Game {teamGameMatchup} was not added.");
        }

        if (hashSet.Count * 2 != regularSeasonMatchups.Sum(kvp => kvp.Value.Count))
        {
            throw new ArgumentException("Failed symmetry check.");
        }
            
        var deduplicatedGames = hashSet.ToList();
        deduplicatedGames.Shuffle(new Random());
        return deduplicatedGames;
    }

    private static List<GameMatchup>[] SeparateGamesIntoWeeks(IEnumerable<GameMatchup> deduplicatedGames)
    {
        var gameList = deduplicatedGames.ToList();
        gameList.Shuffle(new Random());

        return gameList
            .Chunk(20)
            .Select(w => w.ToList())
            .ToArray();
    }

    private static List<GameMatchup>[] MoveGamesForByes(List<GameMatchup>[] weeks)
    {
        var random = new Random();
        var weeksWithWeek17 = new List<GameMatchup>[17];
        Array.Copy(weeks, weeksWithWeek17, 16);
        weeks = weeksWithWeek17;
        weeks[16] = new List<GameMatchup>();
            
        for (int weekIndex = 4; weekIndex <= 12; weekIndex++)
        {
            var byesThisWeek = weekIndex < 11
                ? 2
                : 3;

            for (int i = 0; i < byesThisWeek; i++)
            {
                var gameIndexToMove = random.Next(0, weeks[weekIndex].Count);
                var gameToMove = weeks[weekIndex][gameIndexToMove];
                weeks[weekIndex].RemoveAt(gameIndexToMove);
                weeks[16].Add(gameToMove);
            }
        }

        return weeks;
    }

    private static List<DateTimeOffset> GetRegularSeasonWeekStartDatesForYear(int calendarYear)
    {
        // Week 1 starts on the second Thursday of September
        var weekStartDate = GetNthWeekdayOfMonth(calendarYear, 9, DayOfWeek.Thursday, 2);

        var weekStartDates = new List<DateTimeOffset>
        {
            weekStartDate
        };

        for (int i = 2; i <= 17; i++)
        {
            weekStartDate = weekStartDate.AddDays(7d);
            weekStartDates.Add(weekStartDate);
        }

        return weekStartDates;
    }

    private static DateTimeOffset GetNthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int occurence)
    {
        var date = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var seenOccurences = 0;

        while (seenOccurences < occurence && date.Day < DateTime.DaysInMonth(year, month))
        {
            if (date.DayOfWeek == dayOfWeek)
            {
                seenOccurences += 1;

                if (seenOccurences == occurence) { break; }
            }

            date = date.AddDays(1d);
        }

        if (seenOccurences != occurence)
        {
            throw new ArgumentOutOfRangeException(nameof(occurence),
                $"Asked for occurence #{occurence} of {dayOfWeek} in {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {year}");
        }

        return date;
    }

    private static List<GameMatchup> GetAllMatchupsForSeason(Dictionary<BasicTeamInfo, List<GameMatchup>> games)
    {
        var comparer = new GameMatchupComparer();

        return games
            .SelectMany(kvp => kvp.Value)
            .Distinct(comparer)
            .ToList();
    }
    #endregion

    #region Preseason Generation
    private static IEnumerable<List<(DateTimeOffset gameTime, GameMatchup game)>> GetAllPreseasonMatchupsForSeasonYear(IReadOnlyCollection<BasicTeamInfo> basicTeamInfos,
        IReadOnlyCollection<GameMatchup?> regularSeasonMatchups,
        DateTimeOffset regularSeasonWeek1StartDate)
    {
        var preseasonWeeks = new List<(DateTimeOffset gameTime, GameMatchup game)>[4];
        var preseasonWeekStartDates = GetPreseasonWeekStartDatesForYear(regularSeasonWeek1StartDate);
        var teamInfoBuffer = new List<BasicTeamInfo>();
        var random = new Random();

        for (var i = 0; i < preseasonWeeks.Length; i++)
        {
            preseasonWeeks[i] = new List<(DateTimeOffset gameTime, GameMatchup game)>();
        }

        for (var weekNumber = 0; weekNumber < preseasonWeeks.Length; weekNumber++)
        {
            var week = preseasonWeeks[weekNumber];
            var weekStartDate = preseasonWeekStartDates[weekNumber];
                
            teamInfoBuffer.AddRange(basicTeamInfos);
            GameMatchup? matchup;

            for (int gameNumber = 0; gameNumber < 20; gameNumber++)
            {
                int firstTeamIndex;
                int secondTeamIndex;

                do
                {
                    firstTeamIndex = random.Next(0, teamInfoBuffer.Count);

                    do
                    {
                        secondTeamIndex = random.Next(0, teamInfoBuffer.Count);
                    } while (secondTeamIndex == firstTeamIndex);

                    matchup = new GameMatchup
                    {
                        HomeTeam = teamInfoBuffer[firstTeamIndex],
                        AwayTeam = teamInfoBuffer[secondTeamIndex]
                    };
                } while (regularSeasonMatchups.Any(m => m!.SymmetricallyEquals(matchup)));

                teamInfoBuffer.RemoveAt(Math.Max(firstTeamIndex, secondTeamIndex));
                teamInfoBuffer.RemoveAt(Math.Min(firstTeamIndex, secondTeamIndex));
                
                week.Add((weekStartDate.Add(new TimeSpan(19, 0, 0)), matchup));
            }
        }

        return preseasonWeeks;
    }

    private static List<DateTimeOffset> GetPreseasonWeekStartDatesForYear(DateTimeOffset regularSeasonWeek1StartDate) =>
        new List<DateTimeOffset>
        {
            regularSeasonWeek1StartDate.AddDays(-28d),
            regularSeasonWeek1StartDate.AddDays(-21d),
            regularSeasonWeek1StartDate.AddDays(-14d),
            regularSeasonWeek1StartDate.AddDays(-7d)
        };
    #endregion

    
}