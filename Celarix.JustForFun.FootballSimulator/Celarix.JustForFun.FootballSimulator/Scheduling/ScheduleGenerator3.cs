using System.Diagnostics;
using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using static Celarix.JustForFun.FootballSimulator.Scheduling.DivisionMatchupCycles;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    public sealed class ScheduleGenerator3
    {
        private const int YearZero = 2014;

        private readonly BasicTeamInfo[] teams;

        public ScheduleGenerator3(BasicTeamInfo[] teams)
        {
            if (teams.Length != 40)
            {
                throw new ArgumentException("There must be exactly 40 teams in the league.", nameof(teams));
            }

            this.teams = teams;
        }

        public List<GameRecord> GenerateScheduleForYear(int year,
            Dictionary<BasicTeamInfo, Team> dataTeams,
            Dictionary<BasicTeamInfo, int>? previousSeasonDivisionRankings)
        {
            var cycleYear = (year - YearZero) % 5;
            var teamOpponents = GetTeamOpponentsForSeason(cycleYear, previousSeasonDivisionRankings);
            var scheduledGames = GetRegularSeasonGameRecords(teamOpponents, dataTeams);
            AssignWeekNumbers(scheduledGames);
            ChooseByeGames(scheduledGames);
            ScheduleGamesToTimeslots(scheduledGames, year);
            var preseasonGames = GetPreseasonGamesForYear(year, dataTeams);

            return preseasonGames.Concat(scheduledGames.Select(g => g.GameRecord))
                .OrderBy(g => g.KickoffTime)
                .ToList();
        }

        private List<GameMatchup> GetTeamOpponentsForSeason(int cycleYear,
            Dictionary<BasicTeamInfo, int>? previousSeasonDivisionRankings)
        {
            var comparer = new BasicTeamInfoComparer();
            var matchups = new List<GameMatchup>();
            previousSeasonDivisionRankings ??= GetDefaultPreviousSeasonDivisionRankings();

            foreach (var team in teams)
            {
                // Intradivision: 2 games each against 3 division opponents (6 games total)
                foreach (var intradivisionOpponent in GetTeamsInDivision(team.Conference, team.Division)
                             .Where(t => !comparer.Equals(t, team)))
                {
                    matchups.Add(new GameMatchup
                    {
                        TeamA = team,
                        TeamB = intradivisionOpponent,
                        GameType = ScheduledGameType.IntradivisionalFirstSet,
                        HomeTeamIsTeamA = true
                    });

                    matchups.Add(new GameMatchup
                    {
                        TeamA = team,
                        TeamB = intradivisionOpponent,
                        GameType = ScheduledGameType.IntradivisionalSecondSet,
                        HomeTeamIsTeamA = false
                    });
                }

                // Intraconference: 1 game against each of the 4 teams in another division in the same conference (4 games total)
                var intraconferenceOpponents = GetIntraconferenceOpponentsForTeam(team, cycleYear).GroupBy(o => o.Name).ToArray();

                if (intraconferenceOpponents.Length == 3)
                {
                    var doubleOpponent = intraconferenceOpponents.First(g => g.Count() == 2).First();
                    matchups.Add(new GameMatchup
                    {
                        TeamA = team,
                        TeamB = doubleOpponent,
                        GameType = ScheduledGameType.IntraconferenceFirstSet,
                        HomeTeamIsTeamA = true
                    });
                    matchups.Add(new GameMatchup
                    {
                        TeamA = team,
                        TeamB = doubleOpponent,
                        GameType = ScheduledGameType.IntraconferenceSecondSet,
                        HomeTeamIsTeamA = false
                    });
                    
                    matchups.AddRange(intraconferenceOpponents.Where(g => g.Count() == 1)
                        .Select(g => g.First())
                        .Select((o, i) => new GameMatchup
                        {
                            TeamA = team,
                            TeamB = o,
                            GameType = ScheduledGameType.IntraconferenceFirstSet,
                            HomeTeamIsTeamA = i % 2 == 0
                        }));
                }
                else
                {
                    matchups.AddRange(intraconferenceOpponents.SelectMany(g => g)
                        .Select((o, i) => new GameMatchup
                        {
                            TeamA = team,
                            TeamB = o,
                            GameType = ScheduledGameType.IntraconferenceFirstSet,
                            HomeTeamIsTeamA = i % 2 == 0
                        }));
                }

                // Interconference: 1 game against each of the 4 teams in a division in the other conference (4 games total)
                matchups.AddRange(GetTeamsInDivision(team.Conference.OtherConference(), GetInterconferenceOpponentDivision(cycleYear, team.Conference, team.Division))
                    .Select((o, i) => new GameMatchup
                    {
                        TeamA = team,
                        TeamB = o,
                        GameType = ScheduledGameType.Interconference,
                        HomeTeamIsTeamA = i % 2 == 0
                    }));
                
                // Remaining intraconference: 1 game against 2 teams in 2 other divisions that finished in the same position as the team did (2 games total)
                var remainingIntraconferenceOpponents = GetRemainingIntraconferenceOpponentTeams(team, cycleYear, previousSeasonDivisionRankings).ToArray();

                if (comparer.Equals(remainingIntraconferenceOpponents[0], remainingIntraconferenceOpponents[1]))
                {
                    // We're in the year where the division plays itself for the remaining intraconference games.
                    matchups.Add(new GameMatchup
                    {
                        TeamA = team,
                        TeamB = remainingIntraconferenceOpponents[0],
                        GameType = ScheduledGameType.RemainingIntraconferenceFirstSet,
                        HomeTeamIsTeamA = true
                    });

                    matchups.Add(new GameMatchup
                    {
                        TeamA = team,
                        TeamB = remainingIntraconferenceOpponents[0],
                        GameType = ScheduledGameType.RemainingIntraconferenceSecondSet,
                        HomeTeamIsTeamA = false
                    });
                }
                else
                {
                    matchups.AddRange(remainingIntraconferenceOpponents.Select((o, i) => new GameMatchup
                    {
                        TeamA = team,
                        TeamB = o,
                        GameType = ScheduledGameType.RemainingIntraconferenceFirstSet,
                        HomeTeamIsTeamA = i % 2 == 0
                    }));
                }
            }

            if (matchups.Count != 640)
            {
                throw new InvalidOperationException("Invalid number of matchups.");
            }

            if (matchups.GroupBy(m => m.TeamA).Any(g => g.Count(m => m.HomeTeamIsTeamA) != 8))
            {
                throw new InvalidOperationException("Invalid number of home matchups.");
            }
            
            return matchups;
        }

        private List<ScheduledGame> GetRegularSeasonGameRecords(List<GameMatchup> matchups,
            Dictionary<BasicTeamInfo, Team> dataTeams)
        {
            var gameRecords = new List<ScheduledGame>(320);
            matchups = matchups
                .Distinct(new GameMatchupComparer(new BasicTeamInfoComparer()))
                .OrderBy(m => m.TeamA.Name)
                .ToList();

            if (matchups.Count != 320)
            {
                throw new InvalidOperationException("Invalid number of matchups.");
            }

            if (matchups.Count(m => m.HomeTeamIsTeamA) != 160)
            {
                throw new InvalidOperationException("Invalid number of home matchups.");
            }

            foreach (var matchup in matchups)
            {
                var homeTeam = matchup.HomeTeamIsTeamA ? matchup.TeamA : matchup.TeamB;
                var awayTeam = matchup.HomeTeamIsTeamA ? matchup.TeamB : matchup.TeamA;

                var homeDataTeam = dataTeams[homeTeam];
                var awayDataTeam = dataTeams[awayTeam];
                
                gameRecords.Add(new ScheduledGame
                {
                    HomeTeamInfo = homeTeam,
                    AwayTeamInfo = awayTeam,
                    GameRecord = new GameRecord
                    {
                        GameType = GameType.RegularSeason,
                        AwayTeam = awayDataTeam,
                        StadiumID = homeDataTeam.HomeStadiumID,
                        HomeTeam = homeDataTeam
                    },
                    GameType = matchup.GameType
                });
            }

            if (gameRecords.Count != 320)
            {
                throw new InvalidOperationException("Invalid number of game records.");
            }
            
            return gameRecords;
        }

        private void ChooseByeGames(List<ScheduledGame> scheduledGames)
        {
            // Maybe pick week numbers first, then choose bye games?
            var byeChosenForTeam = teams.ToDictionary(t => t, _ => false);
            var random = new Random();

            scheduledGames.Shuffle(random);

            foreach (var game in scheduledGames.Where(g => g.GameRecord.WeekNumber is >= 3 and <= 13))
            {
                if (byeChosenForTeam[game.HomeTeamInfo] || byeChosenForTeam[game.AwayTeamInfo]) { continue; }
                byeChosenForTeam[game.HomeTeamInfo] = true;
                byeChosenForTeam[game.AwayTeamInfo] = true;

                // Reference types ho! This line also sets the week number in scheduledGames.
                game.GameRecord.WeekNumber = 17;
            }

            if (scheduledGames.Count(g => g.GameRecord.WeekNumber == 17) != 20)
            {
                throw new InvalidOperationException("Invalid number of bye games.");
            }
        }

        private void AssignWeekNumbers(List<ScheduledGame> scheduledGames)
        {
            // TODO: make all Random instances a field
            var random = new Random(1234);
            var comparer = new BasicTeamInfoComparer();
            scheduledGames.Shuffle(random);

            Func<ScheduledGame, int, int> scorer = (game, index) =>
            {
                var weekNumber = index / 20;
                var weekStartIndex = weekNumber * 20;

                var homeTeamAlreadyPlays = false;
                var awayTeamAlreadyPlays = false;

                for (var i = weekStartIndex; i < index; i++)
                {
                    var weekGame = scheduledGames[i];
                    if (comparer.Equals(weekGame.HomeTeamInfo, game.HomeTeamInfo) ||
                        comparer.Equals(weekGame.AwayTeamInfo, game.HomeTeamInfo))
                    {
                        homeTeamAlreadyPlays = true;
                    }

                    if (comparer.Equals(weekGame.HomeTeamInfo, game.AwayTeamInfo) ||
                        comparer.Equals(weekGame.AwayTeamInfo, game.AwayTeamInfo))
                    {
                        awayTeamAlreadyPlays = true;
                    }

                    if (homeTeamAlreadyPlays && awayTeamAlreadyPlays) { return -2; }
                }

                return homeTeamAlreadyPlays || awayTeamAlreadyPlays ? -1 : 0;
            };
            var hillClimber = new BacktrackingHillClimber<ScheduledGame>(scheduledGames, scorer, random);
            var climbingStartTime = DateTimeOffset.Now;
            while (DateTimeOffset.Now - climbingStartTime < TimeSpan.FromSeconds(100))
            {
                for (int i = 0; i < 100; i++)
                {
                    hillClimber.RunClimbStep();
                }
            }

            for (int i = 0; i < 320; i++)
            {
                var weekNumber = i / 20;
                scheduledGames[i].GameRecord.WeekNumber = weekNumber + 1;
            }
        }

        private bool GameTeamsAlreadyPlayThisWeek(ScheduledGame gameToAssign,
            ScheduledGame[] gameSlots,
            int slotIndex,
            BasicTeamInfoComparer comparer)
        {
            var weekNumber = slotIndex / 20;
            var weekStartSlot = weekNumber * 20;
            var weekEndSlot = weekStartSlot + 19;

            for (int i = weekStartSlot; i < weekEndSlot; i++)
            {
                var gameSlot = gameSlots[i];
                if (gameSlot is null) { continue; }
                if (comparer.Equals(gameSlot.HomeTeamInfo, gameToAssign.HomeTeamInfo) ||
                    comparer.Equals(gameSlot.HomeTeamInfo, gameToAssign.AwayTeamInfo) ||
                    comparer.Equals(gameSlot.AwayTeamInfo, gameToAssign.HomeTeamInfo) ||
                    comparer.Equals(gameSlot.AwayTeamInfo, gameToAssign.AwayTeamInfo))
                {
                    return true;
                }
            }

            return false;
        }

        private int NextEmptySlotAfterCurrent(ScheduledGame[] gameSlots, int currentSlotIndex)
        {
            var hasWrappedAround = false;

            do
            {
                currentSlotIndex += 1;
                if (currentSlotIndex >= gameSlots.Length)
                {
                    if (hasWrappedAround)
                    {
                        throw new InvalidOperationException("No empty slot found.");
                    }
                    else
                    {
                        currentSlotIndex = 0;
                        hasWrappedAround = true;
                    }
                }
            } while (gameSlots[currentSlotIndex] != null);

            return currentSlotIndex;
        }

        private int FirstEmptySlot(ScheduledGame[] gameSlots)
        {
            for (int i = 0; i < gameSlots.Length; i++)
            {
                if (gameSlots[i] is null)
                {
                    return i;
                }
            }

            throw new InvalidOperationException("No empty slot found.");
        }

        private void ScheduleGamesToTimeslots(List<ScheduledGame> scheduledGames, int year)
        {
            var gamesByWeek = scheduledGames
                .GroupBy(g => g.GameRecord.WeekNumber)
                .Select(gr => gr.ToList())
                .ToArray();
            var random = new Random();
            var weekStartMidnight = GetNFLKickoffDayForYear(year);

            foreach (var week in gamesByWeek)
            {
                week.Shuffle(random);

                week[0].GameRecord.KickoffTime = GetThursdayNightFootballTimeFromWeekStart(weekStartMidnight);
                week[1].GameRecord.KickoffTime = GetSundayNightFootballTimeFromWeekStart(weekStartMidnight);
                week[2].GameRecord.KickoffTime = GetMondayNightFootballTimeFromWeekStart(weekStartMidnight);
                week[3].GameRecord.KickoffTime = GetSundayDoubleHeaderTimeFromWeekStart(weekStartMidnight);
                week[4].GameRecord.KickoffTime = GetSundayDoubleHeaderTimeFromWeekStart(weekStartMidnight);

                for (int i = 5; i < week.Count; i++)
                {
                    week[i].GameRecord.KickoffTime = GetSundayOnePMTimeFromWeekStart(weekStartMidnight);
                }

                weekStartMidnight = weekStartMidnight.AddDays(7);
            }
        }

        private List<GameRecord> GetPreseasonGamesForYear(int year,
            Dictionary<BasicTeamInfo, Team> dataTeams)
        {
            var preseasonKickoffDay = GetNFLKickoffDayForYear(year).AddDays(-28);
            var teamComparer = new BasicTeamInfoComparer();
            var preseasonOpponents = SymmetricTable<BasicTeamInfo?>.FromRowKeys(teams, 4, teamComparer);
            var random = new Random(year);

            foreach (var team in teams)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (preseasonOpponents[team, i] != null) { continue; }
                    var opponent = teams[random.Next(40)];

                    while (teamComparer.Equals(team, opponent))
                    {
                        opponent = teams[random.Next(40)];
                    }

                    preseasonOpponents[team, i] = opponent;
                }
            }
            
            var preseasonGames = new List<GameRecord>(80);
            var weekStartDay = preseasonKickoffDay;
            var gamesAssignedThisWeek = 0;
            var weekNumber = 1;

            foreach (var team in teams)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (preseasonOpponents[team, i] == null) { continue; }
                    var opponent = preseasonOpponents[team, i]!;
                    var teamIsHomeTeam = i % 2 == 0;
                    var homeTeam = teamIsHomeTeam ? team : opponent;
                    var awayTeam = teamIsHomeTeam ? opponent : team;
                    var homeDataTeam = dataTeams[homeTeam];
                    var awayDataTeam = dataTeams[awayTeam];

                    preseasonGames.Add(new GameRecord
                    {
                        GameType = GameType.Preseason,
                        WeekNumber = weekNumber,
                        HomeTeam = homeDataTeam,
                        AwayTeam = awayDataTeam,
                        StadiumID = homeDataTeam.HomeStadiumID,
                        KickoffTime = gamesAssignedThisWeek switch
                        {
                            0 => weekStartDay.AddHours(19),                               // Thursday 7:00pm
                            >= 1 and <= 15 => weekStartDay.AddDays(3).AddHours(13),       // Sunday 1:00pm
                            <= 17 => weekStartDay.AddDays(3).AddHours(16).AddMinutes(25), // Sunday 4:25pm
                            18 => weekStartDay.AddDays(3).AddHours(20),                   // Sunday 8:00pm
                            19 => weekStartDay.AddDays(4).AddHours(20),                   // Monday 8:00pm
                            _ => throw new InvalidOperationException("Invalid game number.")
                        }
                    });

                    preseasonOpponents.SymmetricallyClear(team, i);
                    gamesAssignedThisWeek += 1;

                    if (gamesAssignedThisWeek == 20)
                    {
                        gamesAssignedThisWeek = 0;
                        weekNumber += 1;
                        weekStartDay = weekStartDay.AddDays(7);
                    }
                }
            }
            
            return preseasonGames;
        }

        private IEnumerable<BasicTeamInfo> GetTeamsInDivision(Conference conference, Division division) =>
            teams.Where(t => t.Conference == conference && t.Division == division);

        private IEnumerable<BasicTeamInfo> GetIntraconferenceOpponentsForTeam(BasicTeamInfo team, int cycleYear)
        {
            var opponentDivision = GetIntraconferenceOpponentDivision(cycleYear, team.Division);
            if (opponentDivision == team.Division)
            {
                // Intraconference games fill 4 slots, but we can't play ourselves.
                // So we need to play 1 game against 2 opponents and 2 games against the third.
                // We'd like to choose semi-randomly. Let's start by seeding an RNG with the cycle year.
                var random = new Random(cycleYear);

                // There are 16 symmetric slots to fill for these teams in this division:
                // Colts   . . . . 
                // Texans  . . . .
                // Jaguars . . . .
                // Titans  . . . .
                // Since there are 16 symmetric slots, we only need to fill 8 of them.
                // How can we choose randomly such that each team plays once against 2 opponents and
                // twice against the third? Well, there is still a symmetry here. Let's say the Colts
                // play twice against the Jaguars. Symmetrically, the Jaguars play twice against the Colts.
                // That leaves us with two teams, the Titans and the Texans, who play against each other twice.
                // Let's start by shuffling the teams using the random number generator.
                var divisionTeams = GetTeamsInDivision(team.Conference, team.Division).ToList();
                divisionTeams.Shuffle(random);

                // Now we have the four teams in a pseudo-random order. We can pair them up.
                // The team at index 0 will play twice against the team at index 1 and the
                // team at index 2 will play twice against the team at index 3.
                var teamIndex = divisionTeams.IndexOf(team);
                var twiceOpponentIndex = teamIndex switch
                {
                    0 => 1,
                    1 => 0,
                    2 => 3,
                    3 => 2,
                    _ => throw new InvalidOperationException("Unreachable: Somehow shuffled a 4-item list and got more than 4 items.")
                };

                yield return divisionTeams[twiceOpponentIndex];
                yield return divisionTeams[twiceOpponentIndex];

                divisionTeams.RemoveAt(twiceOpponentIndex);
                divisionTeams.Remove(team);

                foreach (var remainingOpponent in divisionTeams)
                {
                    yield return remainingOpponent;
                }

                // Ugh. Quite inelegant.
            }
            else
            {
                foreach (var opponentTeam in GetTeamsInDivision(team.Conference, opponentDivision))
                {
                    yield return opponentTeam;
                }
            }
        }

        private IEnumerable<BasicTeamInfo> GetRemainingIntraconferenceOpponentTeams(BasicTeamInfo team,
            int cycleYear,
            IReadOnlyDictionary<BasicTeamInfo, int> previousSeasonDivisionRankings)
        {
            var opponentDivisions = GetRemainingIntraconferenceOpponentDivisions(cycleYear, team.Division);

            if (team.Division != opponentDivisions.Item1)
            {
                var teamsInDivisions = GetTeamsInDivision(team.Conference, opponentDivisions.Item1)
                    .Concat(GetTeamsInDivision(team.Conference, opponentDivisions.Item2));
                var teamRanking = previousSeasonDivisionRankings[team];
                return teamsInDivisions.Where(t => previousSeasonDivisionRankings[t] == teamRanking);
            }
            else
            {
                // A team faces its own division again every five years. #4 plays #1 and #3 plays #2 twice.
                var divisionTeams = GetTeamsInDivision(team.Conference, team.Division).ToArray();

                return Enumerable.Repeat(previousSeasonDivisionRankings[team] switch
                {
                    1 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 4),
                    2 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 3),
                    3 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 2),
                    4 => divisionTeams.First(t => previousSeasonDivisionRankings[t] == 1),
                    _ => throw new InvalidOperationException("Invalid division ranking.")
                }, 2);
            }
        }

        private Dictionary<BasicTeamInfo, int> GetDefaultPreviousSeasonDivisionRankings()
        {
            var random = new Random(-1528635010);
            var rankings = new Dictionary<BasicTeamInfo, int>();
            var conferences = new[] { Conference.AFC, Conference.NFC };
            var divisions = new[] { Division.East, Division.North, Division.South, Division.West, Division.Extra };

            foreach (var conference in conferences)
            {
                foreach (var division in divisions)
                {
                    var divisionTeams = GetTeamsInDivision(conference, division).ToList();
                    divisionTeams.Shuffle(random);

                    for (int i = 0; i < 4; i++)
                    {
                        rankings[divisionTeams[i]] = i + 1;
                    }
                }
            }

            return rankings;
        }

        private static DateTimeOffset GetNFLKickoffDayForYear(int year)
        {
            // The NFL season starts on the Thursday following the first Monday in September.
            // So we need to find the first Monday in September and then add 3 days to it.
            var firstSeptember = new DateTimeOffset(year, 9, 1, 0, 0, 0, TimeSpan.Zero);
            var firstMonday = firstSeptember;
            while (firstMonday.DayOfWeek != DayOfWeek.Monday)
            {
                firstMonday = firstMonday.AddDays(1);
            }

            return firstMonday.AddDays(3);
        }

        private static DateTimeOffset GetThursdayNightFootballTimeFromWeekStart(DateTimeOffset weekStartMidnight) =>
            weekStartMidnight.AddHours(20).AddMinutes(15);

        private static DateTimeOffset GetSundayOnePMTimeFromWeekStart(DateTimeOffset weekStartMidnight) =>
            weekStartMidnight.AddDays(3).AddHours(13);

        private static DateTimeOffset GetSundayDoubleHeaderTimeFromWeekStart(DateTimeOffset weekStartMidnight) =>
            weekStartMidnight.AddDays(3).AddHours(16).AddMinutes(25);

        private static DateTimeOffset GetSundayNightFootballTimeFromWeekStart(DateTimeOffset weekStartMidnight) =>
            weekStartMidnight.AddDays(3).AddHours(20).AddMinutes(15);

        private static DateTimeOffset GetMondayNightFootballTimeFromWeekStart(DateTimeOffset weekStartMidnight) =>
            weekStartMidnight.AddDays(4).AddHours(20).AddMinutes(15);
    }
}
