using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    public sealed class ScheduleGenerator3
    {
        private const int YearZero = 2014;

		private readonly BasicTeamInfo[] teams;
        private readonly IRandomFactory randomFactory;

        public ScheduleGenerator3(BasicTeamInfo[] teams, IRandomFactory randomFactory)
        {
            if (teams.Length != 40)
            {
                throw new ArgumentException("There must be exactly 40 teams in the league.", nameof(teams));
            }

            this.teams = teams;
            this.randomFactory = randomFactory;
        }

        public List<GameRecord> GenerateScheduleForYear(int year,
            Dictionary<BasicTeamInfo, Team> dataTeams,
            Dictionary<BasicTeamInfo, int>? previousSeasonDivisionRankings,
            BasicTeamInfo? previousSuperBowlWinner,
            out IReadOnlyList<TeamScheduleDiagnostics> diagnostics)
        {
            var cycleYear = (year - YearZero) % 5;
            var diagnosticDictionary = dataTeams.ToDictionary(kvp => kvp.Key,
                kvp => new TeamScheduleDiagnostics { Team = kvp.Key });

            Log.Information("Generating schedule for {Year} (cycle year {CycleYear})", year, cycleYear);

            if (previousSuperBowlWinner is null)
            {
                previousSuperBowlWinner = teams[randomFactory.Create(Helpers.SchedulingRandomSeed).Next(0, 40)];
                Log.Information("No previous Super Bowl winner provided; randomly selected {Team} as winner", previousSuperBowlWinner.Name);
            }
            
            var teamOpponents = new TeamOpponentDeterminer(teams, randomFactory).GetTeamOpponentsForSeason(cycleYear,
	            previousSeasonDivisionRankings,
                diagnosticDictionary);
            new HomeTeamAssigner(teams, teamOpponents, randomFactory).AssignHomeTeams();
            var scheduledGames = GetRegularSeasonGameRecords(teamOpponents, dataTeams);
            AssignWeekNumbers(scheduledGames);
            ChooseByeGames(scheduledGames);
            ScheduleGamesToTimeslots(scheduledGames, year, previousSuperBowlWinner);
            var preseasonGames = GetPreseasonGamesForYear(year, dataTeams);

            diagnostics = [.. diagnosticDictionary.Values];
            return [.. preseasonGames.Concat(scheduledGames.Select(g => g.GameRecord)).OrderBy(g => g.KickoffTime)];
        }

        private static List<ScheduledGame> GetRegularSeasonGameRecords(List<GameMatchup> matchups,
            Dictionary<BasicTeamInfo, Team> dataTeams)
        {
            var gameRecords = new List<ScheduledGame>(320);
            matchups = [.. matchups
                .Distinct(new GameMatchupComparer(new BasicTeamInfoComparer()))
                .OrderBy(m => m.TeamA.Name)];

            if (matchups.Count != 320)
            {
                throw new InvalidOperationException("Invalid number of matchups.");
            }

            foreach (var matchup in matchups)
            {
                var homeTeam = (matchup.HomeTeamIsTeamA == true) ? matchup.TeamA : matchup.TeamB;
                var awayTeam = (matchup.HomeTeamIsTeamA == true) ? matchup.TeamB : matchup.TeamA;

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
                        AwayTeamID = awayDataTeam.TeamID,
                        StadiumID = homeDataTeam.HomeStadiumID,
		                HomeTeam = homeDataTeam,
                        HomeTeamID = homeDataTeam.TeamID
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

        private void AssignWeekNumbers(List<ScheduledGame> scheduledGames)
        {
            // Algorithm by Bill Lindley from the Kentucky Open Source Society.
            // We miss you, buddy!
            
            // We create a 16-wide, 320-tall table where the rows represent the 320 games of the season
            // and the columns represent the week number (we'll move bye games later). We shuffle the
            // scheduledGames list first, then assign games starting from the first empty slot down.
            // When scheduling a game, all other games with those two teams are marked Ineligible for
            // that week, and that game is marked PreviouslyAssigned for all other weeks.
            
            // Basic assertions:
            if (scheduledGames?.Count != 320)
			{
				throw new ArgumentException("There must be exactly 320 games in the season.", nameof(scheduledGames));
			}
            
            var random = randomFactory.Create(Helpers.SchedulingRandomSeed);
            var shuffledGames = new List<ScheduledGame>(scheduledGames);
            random.Shuffle(shuffledGames);

            var gameAssignmentTable = new GameWeekSlotType[320, 16];

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
				            throw new InvalidOperationException($"Unhandled game assignment value: {gameAssignmentTable[gameIndex, weekNumber]}");
		            }
	            }
            }
            
            Log.Information("Game assignment statistics:");
            Log.Information("Empty slots: {EmptySlots}", emptySlots);
            Log.Information("Assigned slots: {AssignedSlots}", assignedSlots);
            Log.Information("Previously assigned slots: {PreviouslyAssignedSlots}", previouslyAssignedSlots);
            Log.Information("Ineligible slots: {IneligibleSlots}", ineligibleSlots);
            
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
            
            // And, sorry, Bill, but I think I broke the scheduling too much by reducing the possible
            // number of opponents per season. Anything not assigned to a week here will just be assigned
            // randomly, and some teams will have to play more than once a week.
            foreach (var unscheduledGame in shuffledGames.Where(g => g.GameRecord.WeekNumber == 0))
            {
	            var weekNumber = random.Next(1, 17);
	            Log.Information("Randomly scheduling unscheduled {AwayTeam} @ {HomeTeam} game to week {WeekNumber}",
		            unscheduledGame.AwayTeamInfo.Name, unscheduledGame.HomeTeamInfo.Name, weekNumber);
	            unscheduledGame.GameRecord.WeekNumber = weekNumber;
            }
        }

        private void ChooseByeGames(List<ScheduledGame> scheduledGames)
        {
	        var byeChosenForTeam = teams.ToDictionary(t => t, _ => false);
	        var random = randomFactory.Create(Helpers.SchedulingRandomSeed);

	        random.Shuffle(scheduledGames);

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

        private void ScheduleGamesToTimeslots(List<ScheduledGame> scheduledGames, int year, BasicTeamInfo previousSuperBowlWinner)
        {
			var random = randomFactory.Create(Helpers.SchedulingRandomSeed);
			var gamesByWeek = scheduledGames
                .GroupBy(g => g.GameRecord.WeekNumber)
                .OrderBy(g => g.Key)
                .Select(gr =>
                {
                    var gamesInWeek = gr.ToList();
                    random.Shuffle(gamesInWeek);
                    return gamesInWeek;
                })
                .ToArray();
            var weekStartMidnight = GetNFLKickoffDayForYear(year);
            
            SetSuperBowlWinnerAsKickoffGame(previousSuperBowlWinner, gamesByWeek);

            foreach (var week in gamesByWeek)
            {
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

        private static void SetSuperBowlWinnerAsKickoffGame(BasicTeamInfo previousSuperBowlWinner, IList<ScheduledGame>[] gamesByWeek)
        {
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

				        return;
			        }
		        }
	        }
        }

        private static void SetKickoffTime(GameRecord record, DateTimeOffset kickoffTime)
        {
            if (record.AwayTeam == null || record.HomeTeam == null)
            {
                throw new InvalidOperationException("Team data not loaded from the database.");
            }
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
            var random = randomFactory.Create(Helpers.SchedulingRandomSeed);

            foreach (var team in teams)
            {
	            Log.Information("Generating preseason opponents for {Team}", team.Name);
                for (int i = 0; i < 4; i++)
                {
                    if (preseasonOpponents[team, i] is not null) { continue; }
                    var opponent = teams[random.Next(40)];

                    while (teamComparer.Equals(team, opponent)
                           || preseasonOpponents[opponent, i] is not null)
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
                    if (preseasonOpponents[team, i] is null) { continue; }
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
                        HomeTeamID = homeDataTeam.TeamID,
                        AwayTeam = awayDataTeam,
                        AwayTeamID = awayDataTeam.TeamID,
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

					preseasonGames.Add(preseasonGame);
					gamesAssignedThisWeek += 1;
					preseasonOpponents.SymmetricallyClear(team, i);
					Log.Information("Scheduled preseason game #{GameNumber}: {HomeTeam} vs. {AwayTeam} on {KickoffTime} ({RemainingGames} remain for week)",
	                    preseasonGames.Count, homeTeam.Name, awayTeam.Name, preseasonGame.KickoffTime, 20 - gamesAssignedThisWeek);
                   
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
