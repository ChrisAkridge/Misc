using System;
using System.Collections;
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
        private readonly struct BasicTeamInfo : IComparable<BasicTeamInfo>
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

            /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
            /// <param name="other">An object to compare with this instance.</param>
            /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
            /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
            public int CompareTo(BasicTeamInfo other) => string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        private readonly struct DivisionMatchupsForSeason
        {
            public Division TypeIIOpponentDivision { get; }
            public Division TypeIIIOpponentDivision { get; }
            public Division[] TypeIVOpponentDivisions { get; }

            public DivisionMatchupsForSeason(Division typeIIOpponentDivision, Division typeIIIOpponentDivision,
                Division firstTypeIVOpponentDivision, Division secondTypeIVOpponentDivision)
            {
                TypeIIOpponentDivision = typeIIOpponentDivision;
                TypeIIIOpponentDivision = typeIIIOpponentDivision;
                TypeIVOpponentDivisions = new[]
                {
                    firstTypeIVOpponentDivision,
                    secondTypeIVOpponentDivision
                };
            }
        }
        
        private sealed class GameMatchup
        {
            public BasicTeamInfo AwayTeam { get; init; }
            public BasicTeamInfo HomeTeam { get; init; }
            public int GameType { get; init; }
            public BasicTeamInfo AddedBy { get; init; }
            public bool SelectedForSchedule { get; set; }
            public Guid GamePairId { get; set; }

            /// <summary>Indicates whether this instance and a specified object are equal.</summary>
            /// <param name="obj">The object to compare with the current instance.</param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
            public override bool Equals(object? obj) =>
                obj is GameMatchup that && AwayTeam.Equals(that.AwayTeam) && HomeTeam.Equals(that.HomeTeam) && GameType == that.GameType;

            public bool SymmetricallyEquals(GameMatchup that) =>
                (AwayTeam.Equals(that.AwayTeam) || AwayTeam.Equals(that.HomeTeam))
                && (HomeTeam.Equals(that.HomeTeam) || HomeTeam.Equals(that.AwayTeam))
                && (GameType == that.GameType);

            /// <summary>Returns the hash code for this instance.</summary>
            /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode() => HashCode.Combine(AwayTeam, HomeTeam, GameType);

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() => $"{AwayTeam} @ {HomeTeam} (type {GameType}, added by {AddedBy})";
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

        private sealed class GameMatchupPairComparer : IEqualityComparer<GameMatchup>
        {
            public bool Equals(GameMatchup? x, GameMatchup? y) =>
                ReferenceEquals(x, y)
                || (!ReferenceEquals(x, null)
                    && !ReferenceEquals(y, null)
                    && x.GetType() == y.GetType()
                    && x.GamePairId.Equals(y.GamePairId));

            public int GetHashCode(GameMatchup obj) => obj.GamePairId.GetHashCode();
        }

        private sealed class CyclingGameEnumerator : IEnumerator<GameMatchup>
        {
            private readonly List<GameMatchup> games;
            private int currentGameIndex = -1;

            public CyclingGameEnumerator(IEnumerable<GameMatchup> games)
            {
                this.games = games.ToList();
            }

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public GameMatchup Current => games[currentGameIndex];

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            object IEnumerator.Current => Current;

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            /// <returns>
            /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                AdvanceIndexAndCheckForLoop();

                while (Current.SelectedForSchedule)
                {
                    var loopOccurred = AdvanceIndexAndCheckForLoop();
                    if (loopOccurred && games.All(g => g.SelectedForSchedule)) { return false; }
                }

                return true;

                bool AdvanceIndexAndCheckForLoop()
                {
                    if (currentGameIndex < games.Count - 1)
                    {
                        currentGameIndex += 1;

                        return false;
                    }

                    currentGameIndex = 0;

                    return true;
                }
            }

            /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
            public void Reset() { throw new NotImplementedException(); }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose() { }
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
            var regularSeasonWeeks = GetRegularSeasonTimeslotsForGames(seasonYear, regularSeasonMatchups, basicTeamInfos);
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
                        new DivisionMatchupsForSeason(N, N, E, S),
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
                        new DivisionMatchupsForSeason(N, W, E, S),
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
                    // Also, there's likely the same sort of problem with Type II games when a division
                    // plays itself. One team chooses four others at random, but the four teams it chose
                    // may randomly try to choose it again, leading to us attempting to add a 5th game.
                    // This one's fairly easy - when generating a random team, check to see if we aren't
                    // already playing them.
                    //
                    // okay, actually, no, it's not. Since each team only has 3 division rivals, and
                    // since 4 is not divisble by 3, we're always going to have something of an imbalance.
                    // How about this: let's say the AFC North is playing itself. That's Bengals, Ravens,
                    // Steelers, Browns. What would a valid Type II set of games for all four teams be?
                    //
                    // Bengals:  Ravens,   Ravens,   Steelers, Browns
                    // Ravens:   Bengals,  Bengals,  Browns,   Steelers
                    // Steelers: Browns,   Browns,   Bengals,  Ravens
                    // Browns:   Steelers, Steelers, Ravens,   Bengals
                    //
                    // Okay, this is a valid solution and we'll just use it for every time a division
                    // faces itself in Type II. If the four teams of a division are A, B, C, and D, the
                    // pattern is BBCD-AADC-DDAB-CCBA. There are likely other symmetrical solutions, but,
                    // eh.
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
            Dictionary<BasicTeamInfo, List<GameMatchup>> regularSeasonMatchups,
            IEnumerable<BasicTeamInfo> teams)
        {
            var regularSeasonWeekStartDates = GetRegularSeasonWeekStartDatesForYear(seasonYear);
            var gamesByWeek = SeparateGamesIntoWeeks(DeduplicateMatchups(regularSeasonMatchups), teams);
            var regularSeasonWeeks = new List<(DateTimeOffset gameTime, GameMatchup game)>[17];

            for (var weekNumber = 0; weekNumber < gamesByWeek.Length; weekNumber++)
            {
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

        private static List<GameMatchup> DeduplicateMatchups(Dictionary<BasicTeamInfo, List<GameMatchup>> regularSeasonMatchups)
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

        private static GameMatchup[][] SeparateGamesIntoWeeks(IEnumerable<GameMatchup> deduplicatedGames,
            IEnumerable<BasicTeamInfo> teams)
        {
            var gameList = deduplicatedGames.ToList();
            gameList.Shuffle(new Random());

            return gameList
                .Chunk(16)
                .ToArray();
        }

        private static void MoveGamesForByes(IReadOnlyList<List<GameMatchup?>> weeks)
        {
            var random = new Random();

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

        private static List<DateTimeOffset> GetPreseasonWeekStartDatesForYear(DateTimeOffset regularSeasonWeek1StartDate) =>
            new List<DateTimeOffset>
            {
                regularSeasonWeek1StartDate.AddDays(-28d),
                regularSeasonWeek1StartDate.AddDays(-21d),
                regularSeasonWeek1StartDate.AddDays(-14d),
                regularSeasonWeek1StartDate.AddDays(-7d)
            };
        #endregion

        private static void RemoveGameFromBothTeams(Dictionary<BasicTeamInfo, List<GameMatchup>> games,
            GameMatchup selectedGame)
        {
            games[selectedGame.AwayTeam].RemoveAll(g => g.Equals(selectedGame));
            games[selectedGame.HomeTeam].RemoveAll(g => g.Equals(selectedGame));
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
    }
}
