﻿using System.Text;
using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using static Celarix.JustForFun.FootballSimulator.Scheduling.DivisionMatchupCycles;
using Serilog;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    public sealed class ScheduleGenerator3
    {
        private const int YearZero = 2014;
        private const int RandomSeed = -1039958483;

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
            Dictionary<BasicTeamInfo, int>? previousSeasonDivisionRankings,
            BasicTeamInfo? previousSuperBowlWinner)
        {
            var cycleYear = (year - YearZero) % 5;
            
            Log.Information("Generating schedule for {Year} (cycle year {CycleYear})", year, cycleYear);

            previousSuperBowlWinner ??= teams[new Random(RandomSeed)
	            .Next(0, 40)];
            
            var teamOpponents = GetTeamOpponentsForSeason(cycleYear, previousSeasonDivisionRankings);
            var scheduledGames = GetRegularSeasonGameRecords(teamOpponents, dataTeams);
            AssignWeekNumbers(scheduledGames);
            ChooseByeGames(scheduledGames);
            ScheduleGamesToTimeslots(scheduledGames, year, previousSuperBowlWinner);
            var preseasonGames = GetPreseasonGamesForYear(year, dataTeams);

            // WYLO: we're generating 97 preseason games for 2014, instead of 80
            // also cycle year 3 gives us 118 home games, instead of 120
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

            var rankingsForLogging = string.Join(Environment.NewLine, previousSeasonDivisionRankings
	            .GroupBy(kvp => new
	            {
		            kvp.Key.Conference, kvp.Key.Division
	            })
	            .Select(g =>
	            {
		            var builder = new StringBuilder();
		            builder.Append($"{g.Key.Conference} {g.Key.Division}: ");
		            var teamsInOrder = g.OrderBy(kvp => kvp.Value);

		            foreach (var teamKVP in teamsInOrder) { builder.Append($"#{teamKVP.Value} {teamKVP.Key.Name}, "); }

		            builder.Remove(builder.Length - 2, 2);

		            return builder.ToString();
	            }));
            Log.Information("Previous season division rankings:");
            Log.Information(rankingsForLogging);

            foreach (var team in teams)
            {
                // Intradivision: 2 games each against 3 division opponents (6 games total)
                foreach (var intradivisionOpponent in GetTeamsInDivision(team.Conference, team.Division)
                             .Where(t => !comparer.Equals(t, team)))
                {
                    AddMatchup(matchups, new GameMatchup
                    {
                        TeamA = team,
                        TeamB = intradivisionOpponent,
                        GameType = ScheduledGameType.IntradivisionalFirstSet,
                        HomeTeamIsTeamA = true
                    });

                    AddMatchup(matchups, new GameMatchup
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
                    AddMatchup(matchups, new GameMatchup
                    {
                        TeamA = team,
                        TeamB = doubleOpponent,
                        GameType = ScheduledGameType.IntraconferenceFirstSet,
                        HomeTeamIsTeamA = true
                    });
                    AddMatchup(matchups, new GameMatchup
                    {
                        TeamA = team,
                        TeamB = doubleOpponent,
                        GameType = ScheduledGameType.IntraconferenceSecondSet,
                        HomeTeamIsTeamA = false
                    });

                    var firstSetOpponents = intraconferenceOpponents.Where(g => g.Count() == 1)
	                    .Select(g => g.First())
	                    .Select((o, i) => new GameMatchup
	                    {
		                    TeamA = team,
		                    TeamB = o,
		                    GameType = ScheduledGameType.IntraconferenceFirstSet,
		                    HomeTeamIsTeamA = i % 2 == 0
	                    });

                    foreach (var firstSetOpponent in firstSetOpponents)
                    {
	                    AddMatchup(matchups, firstSetOpponent);
                    }
                }
                else
                {
	                var firstSetOpponents = intraconferenceOpponents.SelectMany(g => g)
		                .Select((o, i) => new GameMatchup
		                {
			                TeamA = team,
			                TeamB = o,
			                GameType = ScheduledGameType.IntraconferenceFirstSet,
			                HomeTeamIsTeamA = i % 2 == 0
		                });

	                foreach (var firstSetOpponent in firstSetOpponents)
	                {
		                AddMatchup(matchups, firstSetOpponent);
	                }
				}

                // Interconference: 1 game against each of the 4 teams in a division in the other conference (4 games total)
                var interconferenceMatchups = GetTeamsInDivision(team.Conference.OtherConference(), GetInterconferenceOpponentDivision(cycleYear, team.Conference, team.Division))
	                .Select((o, i) => new GameMatchup
	                {
		                TeamA = team,
		                TeamB = o,
		                GameType = ScheduledGameType.Interconference,
		                HomeTeamIsTeamA = i % 2 == 0
	                });

                foreach (var interconferenceMatchup in interconferenceMatchups)
                {
	                AddMatchup(matchups, interconferenceMatchup);
                }
                
                // Remaining intraconference: 1 game against 2 teams in 2 other divisions that finished in the same position as the team did (2 games total)
                var remainingIntraconferenceOpponents = GetRemainingIntraconferenceOpponentTeams(team, cycleYear, previousSeasonDivisionRankings).ToArray();

                if (comparer.Equals(remainingIntraconferenceOpponents[0], remainingIntraconferenceOpponents[1]))
                {
                    // We're in the year when the division plays itself for the remaining intraconference games.
                    AddMatchup(matchups, new GameMatchup
                    {
                        TeamA = team,
                        TeamB = remainingIntraconferenceOpponents[0],
                        GameType = ScheduledGameType.RemainingIntraconferenceFirstSet,
                        HomeTeamIsTeamA = true
                    });

                    AddMatchup(matchups, new GameMatchup
                    {
                        TeamA = team,
                        TeamB = remainingIntraconferenceOpponents[0],
                        GameType = ScheduledGameType.RemainingIntraconferenceSecondSet,
                        HomeTeamIsTeamA = false
                    });
                }
                else
                {
	                var remainingIntraconferenceFirstSetMatchups = remainingIntraconferenceOpponents.Select((o, i) => new GameMatchup
	                {
		                TeamA = team,
		                TeamB = o,
		                GameType = ScheduledGameType.RemainingIntraconferenceFirstSet,
		                HomeTeamIsTeamA = i % 2 == 0
	                });

	                foreach (var remainingIntraconferenceFirstSetMatchup in remainingIntraconferenceFirstSetMatchups)
	                {
		                AddMatchup(matchups, remainingIntraconferenceFirstSetMatchup);
	                }
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
        
        private static void AddMatchup(ICollection<GameMatchup> matchups, GameMatchup matchup)
		{
			Log.Information("Adding matchup #{GameNumber}: {TeamA} vs. {TeamB} ({GameType}, home team is {HomeTeam})",
				matchups.Count, matchup.TeamA.Name, matchup.TeamB.Name, matchup.GameType,
				matchup.HomeTeamIsTeamA
					? matchup.TeamA.Name
					: matchup.TeamB.Name);

			matchups.Add(matchup);
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

                var scheduledGame = new ScheduledGame
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
                };
                
                Log.Information("Adding scheduled game #{GameCount}: {HomeTeam} vs. {AwayTeam} ({GameType})",
	                gameRecords.Count, homeTeam.Name, awayTeam.Name, matchup.GameType);
                
                gameRecords.Add(scheduledGame);
            }

            if (gameRecords.Count != 320)
            {
                throw new InvalidOperationException("Invalid number of game records.");
            }
            
            return gameRecords;
        }

        private void ChooseByeGames(List<ScheduledGame> scheduledGames)
        {
            var byeChosenForTeam = teams.ToDictionary(t => t, _ => false);
            var random = new Random(RandomSeed);

            scheduledGames.Shuffle(random);

            foreach (var game in scheduledGames.Where(g => g.GameType is ScheduledGameType.IntradivisionalFirstSet or ScheduledGameType.IntradivisionalSecondSet))
            {
                if (byeChosenForTeam[game.HomeTeamInfo] || byeChosenForTeam[game.AwayTeamInfo]) { continue; }
                byeChosenForTeam[game.HomeTeamInfo] = true;
                byeChosenForTeam[game.AwayTeamInfo] = true;

                // Reference types ho! This line also sets the week number in scheduledGames.
                game.GameRecord.WeekNumber = 17;
                
                Log.Information("Chose bye game for {HomeTeam} and {AwayTeam}", game.HomeTeamInfo.Name, game.AwayTeamInfo.Name);
            }

            if (scheduledGames.Count(g => g.GameRecord.WeekNumber == 17) != 20)
            {
                throw new InvalidOperationException("Invalid number of bye games.");
            }
        }

        private static void AssignWeekNumbers(List<ScheduledGame> scheduledGames)
        {
            // Algorithm by Bill Lindley from the Kentucky Open Source Society.
            // We miss you, buddy!
            
            // We create a 16-wide, 320-tall table where the rows represent the 320 games of the season
            // and the columns represent the week number (we'll move bye games later). We shuffle the
            // scheduledGames list first, then assign games starting from the first empty slot down.
            // When scheduling a game, all other games with those two teams are marked Ineligible for
            // that week, and that game is marked PreviouslyAssigned for all other weeks.
            var debugWriter = new DebugTableWriter();
            
            // Basic assertions:
            if (scheduledGames?.Count != 320)
			{
				throw new ArgumentException("There must be exactly 320 games in the season.", nameof(scheduledGames));
			}
            
            var random = new Random(RandomSeed);
            var shuffledGames = new List<ScheduledGame>(scheduledGames);
            shuffledGames.Shuffle(random);

            var gameAssignmentTable = new GameWeekSlotType[320, 16];
            debugWriter.AddUpdatedTable(gameAssignmentTable, "Begin");

            for (int weekNumber = 0; weekNumber < 16; weekNumber++)
            {
	            var gamesAssignedThisWeek = 0;
	            
	            // We can just walk down the column, assigning any game slot that is Empty.
	            for (int gameIndex = 0; gameIndex < 320; gameIndex++)
	            {
					var assignedGame = shuffledGames[gameIndex];

					if (gameAssignmentTable[gameIndex, weekNumber] != GameWeekSlotType.Empty) { continue; }
		            
					Log.Information("Assigning game #{GameNumber} to week {WeekNumber}: {HomeTeam} vs. {AwayTeam}",
						gameIndex, weekNumber + 1, assignedGame.HomeTeamInfo.Name, assignedGame.AwayTeamInfo.Name);
					
		            gameAssignmentTable[gameIndex, weekNumber] = GameWeekSlotType.Assigned;
		            gamesAssignedThisWeek += 1;

		            var gamesMadeIneligible = 0;
		            for (int gameToMakeIneligibleIndex = gameIndex;
		                 gameToMakeIneligibleIndex < 320;
		                 gameToMakeIneligibleIndex++)
		            {
			            if (gameToMakeIneligibleIndex == gameIndex) { continue; }
			            
			            // It's okay to use gameIndex as the starting index of this loop because
			            // we've already handled the effects of everything above it.
			            var gameToCheckForIneligibility = shuffledGames[gameToMakeIneligibleIndex];

			            var assignedHomeTeam = assignedGame.HomeTeamInfo.Name;
			            var checkedHomeTeam = gameToCheckForIneligibility.HomeTeamInfo.Name;
			            var assignedAwayTeam = assignedGame.AwayTeamInfo.Name;
			            var checkedAwayTeam = gameToCheckForIneligibility.AwayTeamInfo.Name;

			            if (assignedHomeTeam == checkedHomeTeam
			                || assignedHomeTeam == checkedAwayTeam
			                || assignedAwayTeam == checkedHomeTeam
			                || assignedAwayTeam == checkedAwayTeam)
			            {
				            Log.Information("Marking game #{GameNumber} as ineligible for week {WeekNumber}: {HomeTeam} vs. {AwayTeam}",
					            gameToMakeIneligibleIndex, weekNumber + 1, gameToCheckForIneligibility.HomeTeamInfo.Name,
					            gameToCheckForIneligibility.AwayTeamInfo.Name);
				            if (gameAssignmentTable[gameToMakeIneligibleIndex, weekNumber] != GameWeekSlotType.Ineligible)
				            {
					            gamesMadeIneligible += 1;
					            gameAssignmentTable[gameToMakeIneligibleIndex, weekNumber] = GameWeekSlotType.Ineligible;
				            }
			            }
		            }
		            
		            Log.Information("Marked {GamesMadeIneligible} games as ineligible for week {WeekNumber}",
			            gamesMadeIneligible, weekNumber + 1);
		            
		            for (int weekToMarkPreviouslyAssignedIndex = weekNumber + 1; weekToMarkPreviouslyAssignedIndex < 16; weekToMarkPreviouslyAssignedIndex++)
		            {
			            gameAssignmentTable[gameIndex, weekToMarkPreviouslyAssignedIndex] = GameWeekSlotType.PreviouslyAssigned;
		            }
		            
		            debugWriter.AddUpdatedTable(gameAssignmentTable, $"Assigned game #{gameIndex}, {assignedGame.AwayTeamInfo.Name} @ {assignedGame.HomeTeamInfo.Name}");
	            }
	            
	            Log.Information("Assigned {GamesAssignedThisWeek} games for week {WeekNumber}", gamesAssignedThisWeek, weekNumber + 1);
            }
            
            // Do some counting and statistics for logging.
            var emptySlots = 0;
            var assignedSlots = 0;
            var previouslyAssignedSlots = 0;
            var ineligibleSlots = 0;

            for (var weekNumber = 0; weekNumber < 16; weekNumber++)
            {
	            for (var gameIndex = 0; gameIndex < 320; gameIndex++)
	            {
		            switch (gameAssignmentTable[gameIndex, weekNumber])
		            {
			            case GameWeekSlotType.Empty:
				            emptySlots += 1;
				            break;
			            case GameWeekSlotType.Assigned:
				            assignedSlots += 1;
				            break;
			            case GameWeekSlotType.PreviouslyAssigned:
				            previouslyAssignedSlots += 1;
				            break;
			            case GameWeekSlotType.Ineligible:
				            ineligibleSlots += 1;
				            break;
			            default:
				            throw new ArgumentOutOfRangeException();
		            }
	            }
            }
            
            Log.Information("Game assignment statistics:");
            Log.Information("Empty slots: {EmptySlots}", emptySlots);
            Log.Information("Assigned slots: {AssignedSlots}", assignedSlots);
            Log.Information("Previously assigned slots: {PreviouslyAssignedSlots}", previouslyAssignedSlots);
            Log.Information("Ineligible slots: {IneligibleSlots}", ineligibleSlots);

            if (assignedSlots < 320)
            {
	            // Whoops. Missed a few.
	            for (int gameIndex = 0; gameIndex < 320; gameIndex++)
	            {
		            var gameWasAssigned = false;
		            for (int weekIndex = 0; weekIndex < 16; weekIndex++)
		            {
			            if (gameAssignmentTable[gameIndex, weekIndex] == GameWeekSlotType.Assigned)
			            {
				            gameWasAssigned = true;
				            break;
			            }
		            }

		            if (!gameWasAssigned)
		            {
			            var game = shuffledGames[gameIndex];
			            Log.Error("Never assigned game #{GameNumber}! ({AwayTeam} @ {HomeTeam}, {GameType})",
				            gameIndex, game.AwayTeamInfo.Name, game.HomeTeamInfo.Name, game.GameType);
		            }
	            }
            }
            
            // Now we can assign the week numbers to the games.
            for (int weekNumber = 0; weekNumber < 16; weekNumber++)
			{
	            for (int gameIndex = 0; gameIndex < 320; gameIndex++)
	            {
		            if (gameAssignmentTable[gameIndex, weekNumber] == GameWeekSlotType.Assigned)
		            {
			            shuffledGames[gameIndex].GameRecord.WeekNumber = weekNumber + 1;
						Log.Information("Assigned game #{GameNumber} to Week {WeekNumber}",
							gameIndex, weekNumber + 1);
					}
	            }
			}
            
            // debugWriter.WriteHTMLToFile();
        }

        private void ScheduleGamesToTimeslots(List<ScheduledGame> scheduledGames, int year, BasicTeamInfo previousSuperBowlWinner)
        {
            var gamesByWeek = scheduledGames
                .GroupBy(g => g.GameRecord.WeekNumber)
                .Select(gr => gr.ToList())
                .ToArray();
            var random = new Random(RandomSeed);
            var weekStartMidnight = GetNFLKickoffDayForYear(year);
            
            // The NFL regular season kickoff always has the previous Super Bowl winner as host.
            // Find the first home game for the winner in any week and swap it with the first game of the season.
            var comparer = new BasicTeamInfoComparer();

            foreach (var gamesInWeek in gamesByWeek)
            {
	            for (var weekGameIndex = 0; weekGameIndex < gamesInWeek.Count; weekGameIndex++)
	            {
		            var scheduledGame = gamesInWeek[weekGameIndex];

		            if (comparer.Equals(scheduledGame.HomeTeamInfo, previousSuperBowlWinner))
		            {			            
			            var currentFirstGameOfSeason = gamesByWeek[0][0];
			            
			            Log.Information("Rescheduling Week {WeekNumber} game {AwayTeam} @ {HomeTeam} as the season kickoff game",
				            scheduledGame.GameRecord.WeekNumber, scheduledGame.AwayTeamInfo.Name,
				            scheduledGame.HomeTeamInfo.Name);
			            currentFirstGameOfSeason.GameRecord.WeekNumber = scheduledGame.GameRecord.WeekNumber;
			            scheduledGame.GameRecord.WeekNumber = 1;
			            gamesInWeek[weekGameIndex] = currentFirstGameOfSeason;
			            gamesByWeek[0][0] = scheduledGame;
		            }
	            }
            }

            foreach (var week in gamesByWeek)
            {
                week.Shuffle(random);
                
                SetKickoffTime(week[0].GameRecord, GetThursdayNightFootballTimeFromWeekStart(weekStartMidnight));
                SetKickoffTime(week[1].GameRecord, GetSundayNightFootballTimeFromWeekStart(weekStartMidnight));
                SetKickoffTime(week[2].GameRecord, GetMondayNightFootballTimeFromWeekStart(weekStartMidnight));
                SetKickoffTime(week[3].GameRecord, GetSundayDoubleHeaderTimeFromWeekStart(weekStartMidnight));
                SetKickoffTime(week[4].GameRecord, GetSundayDoubleHeaderTimeFromWeekStart(weekStartMidnight));

                for (int i = 5; i < week.Count; i++)
                {
					SetKickoffTime(week[i].GameRecord, GetSundayOnePMTimeFromWeekStart(weekStartMidnight));
                }

                weekStartMidnight = weekStartMidnight.AddDays(7);
            }
        }

        private static void SetKickoffTime(GameRecord record, DateTimeOffset kickoffTime)
        {
	        Log.Information("Setting kickoff time for Week {WeekNumber} game {HomeTeam} vs. {AwayTeam} to {KickoffTime}",
		        record.WeekNumber, record.HomeTeam.TeamName, record.AwayTeam.TeamName, kickoffTime);
	        record.KickoffTime = kickoffTime;
        }

        private List<GameRecord> GetPreseasonGamesForYear(int year,
            Dictionary<BasicTeamInfo, Team> dataTeams)
        {
            var preseasonKickoffDay = GetNFLKickoffDayForYear(year).AddDays(-28);
            
            Log.Information("Preseason for {Year} starts on {PreseasonKickoffDay}", year, preseasonKickoffDay);
            
            var teamComparer = new BasicTeamInfoComparer();
            var preseasonOpponents = SymmetricTable<BasicTeamInfo?>.FromRowKeys(teams, 4, teamComparer);
            var random = new Random(year);

            foreach (var team in teams)
            {
	            Log.Information("Generating preseason opponents for {Team}", team.Name);
                for (int i = 0; i < 4; i++)
                {
                    if (preseasonOpponents[team, i] != null) { continue; }
                    var opponent = teams[random.Next(40)];

                    while (teamComparer.Equals(team, opponent))
                    {
                        opponent = teams[random.Next(40)];
                    }

                    Log.Information("{Team} preseason opponent #{OpponentNumber} is {Opponent}",
	                    team.Name, i + 1, opponent.Name);
                    preseasonOpponents[team, i] = opponent;
                }
            }
            
            var preseasonGames = new List<GameRecord>(80);
            var weekStartDay = preseasonKickoffDay;
            var gamesAssignedThisWeek = 0;
            var weekNumber = 1;

            foreach (var team in teams)
            {
	            Log.Information("Scheduling preseason games for {Team}", team.Name);
                for (int i = 0; i < 4; i++)
                {
                    if (preseasonOpponents[team, i] == null) { continue; }
                    var opponent = preseasonOpponents[team, i]!;
                    var teamIsHomeTeam = i % 2 == 0;
                    var homeTeam = teamIsHomeTeam ? team : opponent;
                    var awayTeam = teamIsHomeTeam ? opponent : team;
                    var homeDataTeam = dataTeams[homeTeam];
                    var awayDataTeam = dataTeams[awayTeam];

                    var preseasonGame = new GameRecord
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
                    };
                    
                    Log.Information("Scheduled preseason game #{GameNumber}: {HomeTeam} vs. {AwayTeam} on {KickoffTime} ({RemainingGames} remain for week)",
	                    preseasonGames.Count, homeTeam.Name, awayTeam.Name, preseasonGame.KickoffTime, 20 - gamesAssignedThisWeek);
                    preseasonGames.Add(preseasonGame);

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
            
            Log.Information("Scheduled {Count} preseason games", preseasonGames.Count);
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
	        Log.Information("No previous season division rankings available, generating default...");
	        
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

            var nflKickoffDayForYear = firstMonday.AddDays(3);
            Log.Information("NFL kickoff day for {Year} is {KickoffDay}", year, nflKickoffDayForYear);

            return nflKickoffDayForYear;
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
