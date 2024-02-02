using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using static Celarix.JustForFun.FootballSimulator.Scheduling.DivisionMatchupCycles;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    internal sealed class ScheduleGenerator3
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

        private SymmetricTable<BasicTeamInfo?> GetTeamOpponentsForSeason(int cycleYear,
            Dictionary<BasicTeamInfo, int>? previousSeasonDivisionRankings)
        {
            var opponents = SymmetricTable<BasicTeamInfo>.FromRowKeys(teams, 16, new BasicTeamInfoComparer());
            previousSeasonDivisionRankings ??= GetDefaultPreviousSeasonDivisionRankings();

            foreach (var team in teams)
            {
                var teamOpponents = new List<BasicTeamInfo>();

                // Intradivision: 2 games each against 3 division opponents (6 games total)
                teamOpponents.AddRange(GetTeamsInDivision(team.Conference, team.Division).Where(t => t != team).RepeatEachItem(2));

                // Intraconference: 1 game against each of the 4 teams in another division in the same conference (4 games total)
                teamOpponents.AddRange(GetIntraconferenceOpponentsForTeam(team, cycleYear));

                // Interconference: 1 game against each of the 4 teams in a division in the other conference (4 games total)
                teamOpponents.AddRange(GetTeamsInDivision(team.Conference.OtherConference(),
                    GetInterconferenceOpponentDivision(cycleYear, team.Conference, team.Division)));

                // Remaining intraconference: 1 game against 2 teams in 2 other divisions that finished in the same position as the team did (2 games total)
                teamOpponents.AddRange(GetRemainingIntraconferenceOpponentTeams(team, cycleYear, previousSeasonDivisionRankings));

                for (var cellNumber = 0; cellNumber < 16; cellNumber++)
                {
                    opponents.SetCellUnlessAlreadySet(team, cellNumber, teamOpponents[cellNumber]);
                }
            }

            return opponents;
        }

        private List<ScheduledGame> GetRegularSeasonGameRecords(SymmetricTable<BasicTeamInfo?> opponents,
            Dictionary<BasicTeamInfo, Team> dataTeams)
        {
            var gameRecords = new List<ScheduledGame>(320);

            foreach (var team in opponents.Keys)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (opponents[team, i] == null) { continue; }
                    var keyTeamIsHome = i % 2 == 0;
                    var homeTeam = keyTeamIsHome ? team! : opponents[team, i]!;
                    var awayTeam = keyTeamIsHome ? opponents[team, i]! : team!;
                    var homeDataTeam = dataTeams[homeTeam];
                    var awayDataTeam = dataTeams[awayTeam];

                    GameRecord gameRecord = new GameRecord
                    {
                        GameType = GameType.RegularSeason,
                        WeekNumber = 0,
                        HomeTeam = homeDataTeam,
                        AwayTeam = awayDataTeam,
                        StadiumID = homeDataTeam.HomeStadiumID
                    };
                    var gameType = i switch
                    {
                        < 5 => ScheduledGameType.Intradivisional,
                        < 9 => ScheduledGameType.Intraconference,
                        < 13 => ScheduledGameType.Interconference,
                        < 16 => ScheduledGameType.RemainingIntraconference,
                        _ => throw new InvalidOperationException("Invalid game type.")
                    };

                    gameRecords.Add(new ScheduledGame
                    {
                        GameRecord = gameRecord,
                        GameType = gameType,
                        HomeTeamInfo = homeTeam,
                        AwayTeamInfo = awayTeam
                    });

                    opponents.SymmetricallyClear(team, i);
                }
            }

            return gameRecords;
        }

        private void ChooseByeGames(List<ScheduledGame> scheduledGames)
        {
            var byeChosenForTeam = new Dictionary<BasicTeamInfo, bool>();
            var intradivisionalGames = scheduledGames.Where(g => g.GameType == ScheduledGameType.Intradivisional).ToList();
            var random = new Random();

            Debug.Assert(intradivisionalGames.Count == 120);

            intradivisionalGames.Shuffle(random);

            foreach (var game in intradivisionalGames)
            {
                if (byeChosenForTeam[game.HomeTeamInfo]) { continue; }
                byeChosenForTeam[game.HomeTeamInfo] = true;
                byeChosenForTeam[game.AwayTeamInfo] = true;

                // Reference types ho! This line also sets the week number in scheduledGames.
                game.GameRecord.WeekNumber = 17;
            }

            Debug.Assert(scheduledGames.Count(g => g.GameRecord.WeekNumber == 17) == 20);
        }

        private void AssignWeekNumbers(List<ScheduledGame> scheduledGames)
        {
            var gameWeeks = SymmetricTable<BasicTeamInfo?>.FromRowKeys(teams, 16, new BasicTeamInfoComparer());
            var random = new Random();

            // Step 1: Choose where each week 17 game was "taken" from to create a bye.
            foreach (var week17Game in scheduledGames.Where(g => g.GameRecord.WeekNumber == 17))
            {
                int gameTakenFromWeek;
                do
                {
                    gameTakenFromWeek = random.Next(3, 12);
                } while (gameWeeks.CountColumn(gameTakenFromWeek, c => c != null) == 4);

                gameWeeks[week17Game.HomeTeamInfo, gameTakenFromWeek] = week17Game.AwayTeamInfo;
            }

            // Step 2: Assign the remaining week numbers.
            for (int weekNumber = 0; weekNumber < 16; weekNumber++)
            {
                foreach (var team in teams)
                {
                    if (gameWeeks[team, weekNumber] != null) { continue; }
                    var firstFreeGame = scheduledGames.First(g => g.HomeTeamInfo == team && g.GameRecord.WeekNumber == 0);
                    firstFreeGame.GameRecord.WeekNumber = weekNumber + 1;
                    gameWeeks[team, weekNumber] = firstFreeGame.AwayTeamInfo;
                }
            }
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
            var teamsInDivisions = GetTeamsInDivision(team.Conference, opponentDivisions.Item1)
                .Concat(GetTeamsInDivision(team.Conference, opponentDivisions.Item2));
            var teamRanking = previousSeasonDivisionRankings[team];
            var matchingRankTeams = teamsInDivisions.Where(t => previousSeasonDivisionRankings[t] == teamRanking);

            return matchingRankTeams;
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
