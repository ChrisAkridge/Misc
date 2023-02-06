﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator
{
    /*internal*/ public static class ScheduleGenerator
    {
        // No, this isn't right.
        //
        // 1. Determine the second Thursday in September of the given year. This
        //    becomes the start date of the regular season Week 1.
        // 2. Figure out the opponents for each team across 16 regular season games,
        //    as well as whether the game is at home or away. The game types are
        //    as follows:
        //      - Type I games: Two games against the three division rivals, one
        //        home and one on the road. Six games total.
        //      - Type II games: Four games chosen against all the teams in another
        //        division from the same conference. Two games are at home, two
        //        on the road. Since each conference has 5 teams, one division each
        //        season must face itself again, and since a team cannot play itself,
        //        it gets a fourth game with a random team in its own division.
        //        The division changes on a five-year cycle:
        //          - First year:  East/West,   North/South, Extra plays self
        //          - Second year: East/South,  North/Extra, West plays self
        //          - Third year:  East/North,  West/Extra,  South plays self
        //          - Fourth year: East/Extra,  West/South,  North plays self
        //          - Fifth year:  Extra/North, West/South,  East plays self
        //      - Type III games: Four games against all the teams in a division
        //        from the other conference. Two games are at home, two on the road.
        //        The division changes on a five-year cycle:
        //          - AFC East:  North, East,  West,  South, Extra
        //          - AFC North: South, West,  East,  Extra, North
        //          - AFC South: East,  South, Extra, North, West
        //          - AFC West:  West,  Extra, North, East,  South
        //          - AFC Extra: Extra, North, South, West,  East
        //          
        //          - NFC North: East,  Extra, West,   South, North
        //          - NFC East:  South, East,  North,  West,  Extra
        //          - NFC West:  West,  North, East,   Extra, South
        //          - NFC South: North, South, Extra,  East,  West
        //          - NFC Extra: Extra, West,  South,  North, East
        //      - Type IV Games: Two games. Since Type II games choose from another
        //        division, there are three divisions left in the conference beside
        //        the one this team is in. We always ignore the Extra division here,
        //        leading to two other conferences. Type IV games are against teams
        //        that finished in the same position in the last season in those two
        //        conferences. One game is at home and one is on the road. If no
        //        previous seasons are available, the two teams are chosen from
        //        these two conferences at random.
        // 3. A table is made - 40 columns (1 per team) and 16 rows (1 per game).
        //    Each cell represents an opponent team and whether the game is home
        //    or away. When a cell is filled in, the corresponding cell for the
        //    opponent team is filled in with the opposite home/away value.
        // 4. We also create a small map between a team and its division so we can
        //    quickly determine that.
        // 5. For each team,
        //    1. We fill in the Type I games in the first six slots. The first three
        //       are at home and the next three are away. If the slot is already
        //       filled in, we just skip over this cell.
        //    2. We fill in the Type II games. Given the year, we query for the
        //       Type II opponent division and fill in the next four slots for the
        //       team, again skipping over cells if it already has a value. The
        //       first two slots are home games, the other two are away games.
        //    3. We fill in the Type III games. Mostly the same as the Type II games:
        //       find opponent, fill next 4 slots with opponent teams, first 2 home,
        //       next 2 away.
        //    4. We fill in the Type IV games. The caller must provide us either
        //       with a dictionary mapping team names to their finishing positions
        //       in the last season (string => int) or with null, indicating that
        //       this is the first season and we have no previous record. We then
        //       compute the divisions we should select from (not Extra, ours, or
        //       the one we face Type II games with), pick the teams that finished
        //       in the same spot as us, then fill in the games. The first game is
        //       at home and the other is away.
        // 6. We must now create slots for each game of the regular season. The
        //    season runs for 17 weeks. Teams get a bye week once between Week 4
        //    and Week 12, and the entire regular season is 320 games in total.
        //    This is an average of 18.82 games per week, and since every team
        //    plays in all of Weeks 1-4 and 14-17, that makes for 160 games in non-bye
        //    weeks. This leaves 160 games for the other 9 weeks, 20 fewer games
        //    than if each team played every week. Splitting 20 missing games over
        //    9 weeks gives us something like 2 byes per week for the first 7 weeks
        //    and 3 per week for the final 2 - this is 4 teams and 6 teams with
        //    byes each week.
        //
        //    Each week consists of 3 days of football played: Thursday, Sunday,
        //    and Monday. Each day gets 1 primetime game at 8:15pm and Sunday gets
        //    2 afternoon games at 4:25pm. The remaining games all occur at 1:00pm
        //    on Sunday - this makes for 15 1:00pm games in non-bye weeks, 13
        //    1:00pm games in 2-bye weeks, and 12 1:00pm games in 3-bye weeks.
        //
        //    Week 1 of the regular season begins on the second Thursday in September.
        //
        //    We create an array of dictionaries. Each dictionary is 1 week of
        //    regular-season football. The dictionary has DateTimeOffset keys
        //    and game info values, where game info is just the away team and
        //    home team. A game is chosen at random from the table in step 3
        //    and removed from both slots. The key-value pairs are assigned in this
        //    order: Thursday Night Football, all 12/13/15 1:00pm Sunday games,
        //    the 2 Sunday afternoon games, Sunday Night Football, then Monday
        //    Night Football.
        // 7. With the regular season generated, we can now generate the preseason,
        //    which relies on the regular schedule. The preseason is 4 weeks long
        //    and, here, will be scheduled with the same timeslots as the regular
        //    season - Thursday, Sunday, and Monday games. Each team plays 4 games
        //    for a total of 80 games (15 1:00pm Sunday games) and are chosen at
        //    random (opponents and who is at home) EXCEPT no matchup that occurs
        //    in the regular season can occur in the preseason.
        // 8. Finally, we concatenate the two dictionaries of games into one, sort
        //    it by date ascending, build GameRecord objects out of them, then return
        //    that.
        private readonly struct BasicTeamInfo
        {
            public string Name { get; init; }
            public Conference Conference { get; init; }
            public Division Division { get; init; }

            /// <summary>Indicates whether this instance and a specified object are equal.</summary>
            /// <param name="obj">The object to compare with the current instance.</param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
            public override bool Equals(object? obj) => obj is BasicTeamInfo other && Name == other.Name;

            /// <summary>Returns the hash code for this instance.</summary>
            /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode() => Name.GetHashCode();

            /// <summary>Returns the fully qualified type name of this instance.</summary>
            /// <returns>The fully qualified type name.</returns>
            public override string ToString() => Name;
        }

        private sealed class GameMatchup
        {
            public BasicTeamInfo AwayTeam { get; init; }
            public BasicTeamInfo HomeTeam { get; init; }
            public int GameType { get; init; }

            /// <summary>Indicates whether this instance and a specified object are equal.</summary>
            /// <param name="obj">The object to compare with the current instance.</param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
            public override bool Equals(object? obj) =>
                obj is GameMatchup matchup && AwayTeam.Equals(matchup.AwayTeam) && HomeTeam.Equals(matchup.HomeTeam);

            public bool SymmetricallyEquals(GameMatchup that) =>
                (AwayTeam.Equals(that.AwayTeam) || AwayTeam.Equals(that.HomeTeam))
                && (HomeTeam.Equals(that.HomeTeam) || HomeTeam.Equals(that.AwayTeam));

            /// <summary>Returns the hash code for this instance.</summary>
            /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode() => 17 ^ AwayTeam.GetHashCode() ^ HomeTeam.GetHashCode();

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() => $"{AwayTeam} @ {HomeTeam}";
        }

        private sealed class GameMatchupComparer : IEqualityComparer<GameMatchup>
        {
            public bool Equals(GameMatchup? x, GameMatchup? y) =>
                ReferenceEquals(x, y)
                || (!ReferenceEquals(x, null)
                    && !ReferenceEquals(y, null)
                    && x.GetType() == y.GetType()
                    && (x.AwayTeam.Equals(y.AwayTeam) || x.AwayTeam.Equals(y.HomeTeam))
                    && (x.HomeTeam.Equals(y.HomeTeam) || x.HomeTeam.Equals(y.AwayTeam)));

            public int GetHashCode(GameMatchup obj) => HashCode.Combine(obj.AwayTeam, obj.HomeTeam);
        }

        public static List<GameRecord> GetPreseasonAndRegularSeasonGamesForSeason(int seasonYear,
            List<Team> teams,
            Dictionary<string, int>? previousSeasonTeamPositions)
        {
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

        private static Dictionary<BasicTeamInfo, List<GameMatchup>> GetAllRegularSeasonMatchupsForSeasonYear(List<BasicTeamInfo> basicTeamInfos,
            int seasonYear,
            IReadOnlyDictionary<string, int>? previousSeasonTeamPositions)
        {
            var regularSeasonMatchups = basicTeamInfos.ToDictionary(i => i, _ => new List<GameMatchup>(16));
            var random = new Random();

            foreach (var team in basicTeamInfos)
            {
                if (regularSeasonMatchups[team].Count(g => g.GameType == 0) < 6)
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
                            AwayTeam = otherDivisionTeam, HomeTeam = team, GameType = 1
                        });

                        AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                        {
                            AwayTeam = team, HomeTeam = otherDivisionTeam, GameType = 1
                        });
                    }
                }

                var typeIIOpponentDivision = GetTypeIIGameOpponentDivision(team.Division, seasonYear);
                if (regularSeasonMatchups[team].Count(g => g.GameType == 1) < 4)
                {
                    // Type II games: intraconference games (4 games)
                    var typeIIOpponentTeams =
                        basicTeamInfos
                            .Where(t => t.Conference == team.Conference
                                && t.Division == typeIIOpponentDivision
                                && t.Name != team.Name)
                            .ToList();

                    if (typeIIOpponentTeams.Count == 3) { typeIIOpponentTeams.Add(typeIIOpponentTeams[random.Next(0, 3)]); }

                    for (int i = 0; i < 4; i++)
                    {
                        AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                        {
                            AwayTeam = i < 2 ? typeIIOpponentTeams[i] : team, HomeTeam = i < 2 ? team : typeIIOpponentTeams[i], GameType = 2
                        });
                    }
                }
                
                if (regularSeasonMatchups[team].Count(g => g.GameType == 3) < 4)
                {
                    // Type III games: interconference games (4 games)
                    var typeIIIOpponentDivision =
                        GetTypeIIIGameOpponentDivision(team.Conference, team.Division, seasonYear);
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
                            GameType = 3
                        });
                    }
                }

                if (regularSeasonMatchups[team].Count(g => g.GameType == 4) < 4)
                {
                    // Type IV games: other divisions based on standings
                    var typeIVOpponentDivisions = GetTypeIVGameOpponentDivisions(team.Division, typeIIOpponentDivision);
                    var typeIVOpponentTeams = new BasicTeamInfo[2];

                    if (team.Division == Division.Extra)
                    {
                        // Other divisions are accounted for, so Extra, you have to
                        // play even MORE divisional games!
                        typeIVOpponentTeams = basicTeamInfos
                            .Where(t => t.Conference == team.Conference
                                && t.Division == team.Division
                                && t.Name != team.Name)
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(2)
                            .ToArray();
                    }
                    else if (previousSeasonTeamPositions == null)
                    {
                        // No previous season info, choose two teams at random.
                        typeIVOpponentTeams[0] = basicTeamInfos
                            .Where(t => t.Conference == team.Conference && t.Division == typeIVOpponentDivisions[0])
                            .ElementAt(random.Next(0, 4));

                        typeIVOpponentTeams[1] = basicTeamInfos
                            .Where(t => t.Conference == team.Conference && t.Division == typeIVOpponentDivisions[1])
                            .ElementAt(random.Next(0, 4));
                    }
                    else
                    {
                        typeIVOpponentTeams[0] = basicTeamInfos
                            .Single(t => t.Conference == team.Conference
                                && t.Division == typeIVOpponentDivisions[0]
                                && previousSeasonTeamPositions[t.Name] == previousSeasonTeamPositions[team.Name]);

                        typeIVOpponentTeams[1] = basicTeamInfos
                            .Single(t => t.Conference == team.Conference
                                && t.Division == typeIVOpponentDivisions[1]
                                && previousSeasonTeamPositions[t.Name] == previousSeasonTeamPositions[team.Name]);
                    }

                    // WYLO: Type IV games are all screwed up. They assume that there are only two
                    // remaining divisions (self and the type II opponent excluded), but with Extra
                    // and Type II able to play itself, sometimes there are 3 candidates for Type IV
                    // games. One team chooses two and another team chooses a different two.
                    //
                    // Ideally, there'd be some symmetry. Maybe we figure out all division opponents
                    // first and assign them in the same table format as before. Type I always is self,
                    // and the others would assign themselves to two table cells at once.
                    //
                    // Also, there's likely the same sort of problem with Type II games when a division
                    // plays itself. One team chooses four others at random, but the four teams it chose
                    // may randomly try to choose it again, leading to us attempting to add a 5th game.
                    // This one's fairly easy - when generating a random team, check to see if we aren't
                    // already playing them.
                    AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                    {
                        AwayTeam = typeIVOpponentTeams[0], HomeTeam = team, GameType = 4
                    });

                    AssignGameToBothTeams(regularSeasonMatchups, new GameMatchup
                    {
                        AwayTeam = team, HomeTeam = typeIVOpponentTeams[1], GameType = 4
                    });
                }
            }

            return regularSeasonMatchups;
        }
        
        private static IEnumerable<List<(DateTimeOffset gameTime, GameMatchup game)>> GetRegularSeasonTimeslotsForGames(int seasonYear,
            Dictionary<BasicTeamInfo, List<GameMatchup>> regularSeasonMatchups)
        {
            var regularSeasonWeeks = new List<(DateTimeOffset gameTime, GameMatchup game)>[17];
            var regularSeasonWeekStartDates = GetRegularSeasonWeekStartDatesForYear(seasonYear);
            var random = new Random();

            for (var weekNumber = 0; weekNumber < regularSeasonWeeks.Length; weekNumber++)
            {
                var week = regularSeasonWeeks[weekNumber];
                var weekStartDate = regularSeasonWeekStartDates[weekNumber];

                // Fill in the primetime games.
                var eightThirtyPM = new TimeSpan(20, 30, 0);

                var primetimeGameTimes = new[]
                {
                    weekStartDate.Add(eightThirtyPM), weekStartDate.AddDays(3).Add(eightThirtyPM),
                    weekStartDate.AddDays(4).Add(eightThirtyPM)
                };

                week.AddRange(primetimeGameTimes.Select(t => (t, FindGameForTimeslot(regularSeasonMatchups, random))));

                // Fill in the 2 4:30pm games.
                var sundayAtFourThirtyPM = weekStartDate.AddDays(3).Add(new TimeSpan(16, 30, 0));
                week.Add((sundayAtFourThirtyPM, FindGameForTimeslot(regularSeasonMatchups, random)));
                week.Add((sundayAtFourThirtyPM, FindGameForTimeslot(regularSeasonMatchups, random)));

                var byeCount = (weekNumber + 1) is <= 4 or >= 14
                    ? 0
                    : (weekNumber + 1) < 12
                        ? 2
                        : 3;
                var onePMGameCount = 15 - byeCount;
                var sundayAtOnePM = weekStartDate.AddDays(3).Add(new TimeSpan(13, 0, 0));

                for (int j = 0; j < onePMGameCount; j++)
                {
                    week.Add((sundayAtOnePM, FindGameForTimeslot(regularSeasonMatchups, random)));
                }
            }

            return regularSeasonWeeks;
        }

        private static IEnumerable<List<(DateTimeOffset gameTime, GameMatchup game)>> GetAllPreseasonMatchupsForSeasonYear(IReadOnlyCollection<BasicTeamInfo> basicTeamInfos,
            IReadOnlyCollection<GameMatchup?> regularSeasonMatchups,
            DateTimeOffset regularSeasonWeek1StartDate)
        {
            var preseasonWeeks = new List<(DateTimeOffset gameTime, GameMatchup game)>[4];
            var preseasonWeekStartDates = GetPreseasonWeekStartDatesForYear(regularSeasonWeek1StartDate);
            var teamInfoBuffer = new List<BasicTeamInfo>();
            var random = new Random();

            for (var weekNumber = 0; weekNumber < preseasonWeeks.Length; weekNumber++)
            {
                var week = preseasonWeeks[weekNumber];
                var weekStartDate = preseasonWeekStartDates[weekNumber];
                
                teamInfoBuffer.AddRange(basicTeamInfos);
                GameMatchup? matchup;
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

            return preseasonWeeks;
        }

        private static void AssignGameToBothTeams(IReadOnlyDictionary<BasicTeamInfo, List<GameMatchup>> games, GameMatchup game)
        {
            if (!games[game.AwayTeam].Any(g => g.Equals(game)))
            {
                games[game.AwayTeam].Add(game);
            }

            if (!games[game.HomeTeam].Any(g => g.Equals(game)))
            {
                games[game.HomeTeam]
                    .Add(new GameMatchup
                    {
                        AwayTeam = game.AwayTeam, HomeTeam = game.HomeTeam
                    });
            }
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

        private static List<DateTimeOffset> GetPreseasonWeekStartDatesForYear(DateTimeOffset regularSeasonWeek1StartDate) =>
            new List<DateTimeOffset>
            {
                regularSeasonWeek1StartDate.AddDays(-28d),
                regularSeasonWeek1StartDate.AddDays(-21d),
                regularSeasonWeek1StartDate.AddDays(-14d),
                regularSeasonWeek1StartDate.AddDays(-7d)
            };

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

        private static Division GetTypeIIGameOpponentDivision(Division thisDivision, int seasonYear) =>
            ((seasonYear - 2014) % 5) switch
            {
                0 => thisDivision switch
                {
                    Division.North => Division.South,
                    Division.South => Division.North,
                    Division.East => Division.West,
                    Division.West => Division.Extra,
                    Division.Extra => Division.Extra,
                    _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                },
                1 => thisDivision switch
                {
                    Division.North => Division.Extra,
                    Division.South => Division.East,
                    Division.East => Division.South,
                    Division.West => Division.West,
                    Division.Extra => Division.North,
                    _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                },
                2 => thisDivision switch
                {
                    Division.North => Division.East,
                    Division.South => Division.South,
                    Division.East => Division.North,
                    Division.West => Division.Extra,
                    Division.Extra => Division.West,
                    _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                },
                3 => thisDivision switch
                {
                    Division.North => Division.North,
                    Division.South => Division.West,
                    Division.East => Division.Extra,
                    Division.West => Division.South,
                    Division.Extra => Division.Extra,
                    _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                },
                4 => thisDivision switch
                {
                    Division.North => Division.Extra,
                    Division.South => Division.West,
                    Division.East => Division.East,
                    Division.West => Division.South,
                    Division.Extra => Division.North,
                    _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                },
                _ => throw new InvalidOperationException()
            };

        private static Division GetTypeIIIGameOpponentDivision(Conference thisConference, Division thisDivision, int seasonYear) =>
            ((seasonYear - 2014) % 5) switch
            {
                0 => thisConference switch
                {
                    Conference.AFC => thisDivision switch
                    {
                        Division.North => Division.South,
                        Division.South => Division.East,
                        Division.East => Division.North,
                        Division.West => Division.West,
                        Division.Extra => Division.Extra,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    Conference.NFC => thisDivision switch
                    {
                        Division.North => Division.East,
                        Division.South => Division.North,
                        Division.East => Division.South,
                        Division.West => Division.West,
                        Division.Extra => Division.Extra,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(thisConference), thisConference, null)
                },
                1 => thisConference switch
                {
                    Conference.AFC => thisDivision switch
                    {
                        Division.North => Division.West,
                        Division.South => Division.South,
                        Division.East => Division.East,
                        Division.West => Division.Extra,
                        Division.Extra => Division.North,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    Conference.NFC => thisDivision switch
                    {
                        Division.North => Division.Extra,
                        Division.South => Division.South,
                        Division.East => Division.East,
                        Division.West => Division.North,
                        Division.Extra => Division.West,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(thisConference), thisConference, null)
                },
                2 => thisConference switch
                {
                    Conference.AFC => thisDivision switch
                    {
                        Division.North => Division.East,
                        Division.South => Division.Extra,
                        Division.East => Division.West,
                        Division.West => Division.North,
                        Division.Extra => Division.South,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    Conference.NFC => thisDivision switch
                    {
                        Division.North => Division.West,
                        Division.South => Division.Extra,
                        Division.East => Division.North,
                        Division.West => Division.East,
                        Division.Extra => Division.South,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(thisConference), thisConference, null)
                },
                3 => thisConference switch
                {
                    Conference.AFC => thisDivision switch
                    {
                        Division.North => Division.Extra,
                        Division.South => Division.North,
                        Division.East => Division.South,
                        Division.West => Division.East,
                        Division.Extra => Division.West,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    Conference.NFC => thisDivision switch
                    {
                        Division.North => Division.South,
                        Division.South => Division.East,
                        Division.East => Division.West,
                        Division.West => Division.Extra,
                        Division.Extra => Division.North,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(thisConference), thisConference, null)
                },
                4 => thisConference switch
                {
                    Conference.AFC => thisDivision switch
                    {
                        Division.North => Division.North,
                        Division.South => Division.West,
                        Division.East => Division.Extra,
                        Division.West => Division.South,
                        Division.Extra => Division.East,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    Conference.NFC => thisDivision switch
                    {
                        Division.North => Division.North,
                        Division.South => Division.West,
                        Division.East => Division.Extra,
                        Division.West => Division.South,
                        Division.Extra => Division.East,
                        _ => throw new ArgumentOutOfRangeException(nameof(thisDivision), thisDivision, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(thisConference), thisConference, null)
                },
                _ => throw new InvalidOperationException()
            };

        private static Division[] GetTypeIVGameOpponentDivisions(Division thisDivision, Division typeIIOpponentDivision)
        {
            //if (typeIIOpponentDivision != Division.Extra)
            //{
            return new[]
                {
                    Division.North, Division.South, Division.West, Division.East
                }.Where(d => d != thisDivision && d != typeIIOpponentDivision)
                .Take(2)
                .ToArray();
            // }
            
            //// If we're playing Extra as Type II, the other 3 divisions aren't.
            //// Find the 2 that have us as part of their Type IV opponents.
            //var otherNonExtraDivisions = new[]
            //{
            //    Division.North, Division.South, Division.East, Division.West
            //}.Where(d => d != thisDivision);
            //var otherDivisionTypeIIOpponents =
            //    otherNonExtraDivisions.ToDictionary(d => d, d => GetTypeIIGameOpponentDivision(d, seasonYear));
            //var otherDivisionTypeIVOpponents =
            //    otherDivisionTypeIIOpponents.ToDictionary(d => d.Key, d => new
            //    {
            //        TypeIIOpponent = d.Value,
            //        TypeIVOpponent = GetTypeIVGameOpponentDivisions(d.Key, d.Value, seasonYear)
            //    });

            //return otherDivisionTypeIVOpponents
            //    .Where(kvp => kvp.Value.TypeIVOpponent.Any(o => thisDivision == o))
            //    .Select(kvp => kvp.Key)
            //    .ToArray();
        }

        private static void RemoveGameFromBothTeams(Dictionary<BasicTeamInfo, List<GameMatchup>> games,
            GameMatchup selectedGame)
        {
            games[selectedGame.AwayTeam].RemoveAll(g => g.SymmetricallyEquals(selectedGame));
            games[selectedGame.HomeTeam].RemoveAll(g => g.SymmetricallyEquals(selectedGame));
        }

        private static GameMatchup FindGameForTimeslot(Dictionary<BasicTeamInfo, List<GameMatchup>> games, Random random)
        {
            GameMatchup? selectedGame = null;

            while (selectedGame == null)
            {
                var randomTeam = games.ElementAt(random.Next(0, games.Count));
                var lastSelectedGameIndex = random.Next(0, randomTeam.Value.Count);
                selectedGame = randomTeam.Value[lastSelectedGameIndex];
            }

            RemoveGameFromBothTeams(games, selectedGame);
            return selectedGame;
        }

        private static List<GameMatchup> GetAllMatchupsForSeason(Dictionary<BasicTeamInfo, List<GameMatchup>> games)
        {
            var comparer = new GameMatchupComparer();

            return games
                .SelectMany(kvp => kvp.Value)
                .Distinct(comparer)
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
    }
}
