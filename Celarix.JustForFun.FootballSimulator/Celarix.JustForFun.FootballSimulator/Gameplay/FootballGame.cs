using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using static Celarix.JustForFun.FootballSimulator.Helpers;

namespace Celarix.JustForFun.FootballSimulator.Gameplay
{
    internal sealed class FootballGame
    {
        private readonly FootballContext context;
        private readonly GameRecord currentGameRecord;
        private readonly Random random;

        private InGameTeamStrengths homeTeamStrengths;
        private InGameTeamStrengths awayTeamStrengths;

        private GameClock clock;
        private GameTeam currentKickingTeam;
        private NextPlay nextPlay;

        private List<string> debugDecisions = new List<string>();

        private Team HomeTeam => currentGameRecord.HomeTeam;
        private Team AwayTeam => currentGameRecord.AwayTeam;

        private int AwayScore =>
            currentGameRecord
                .QuarterBoxScores
                .Where(q => q.Team == GameTeam.Away)
                .Sum(q => q.Score);

        private int HomeScore =>
            currentGameRecord
                .QuarterBoxScores
                .Where(q => q.Team == GameTeam.Home)
                .Sum(q => q.Score);

        public bool GameOver =>
            (clock.PeriodNumber >= 4 && HomeScore != AwayScore)
            || (clock.PeriodNumber == 5 && HomeScore == AwayScore && currentGameRecord.WeekNumber <= 17);

        public string StatusMessage { get; private set; }
        
        public bool DebugDecisionModeActive { get; set; }

        private NextPlay NextPlay
        {
            get => nextPlay;
            set
            {
                AddDebugMessage(GetNextPlayDebugMessage(value));
                nextPlay = value;
            }
        }

        public FootballGame(FootballContext context, GameRecord currentGameRecord)
        {
            this.context = context;
            this.currentGameRecord = currentGameRecord;
            random = new Random();
            
            homeTeamStrengths = InGameTeamStrengths.FromTeam(currentGameRecord.HomeTeam);
            awayTeamStrengths = InGameTeamStrengths.FromTeam(currentGameRecord.AwayTeam);
            
            clock = new GameClock();

            DebugDecisionModeActive = true;
        }

        public void RunNextAction()
        {
            debugDecisions.Clear();
            var clockEvent = clock.LastClockEvent;

            if (clockEvent == ClockEvent.NewCoinTossRequired)
            {
                currentKickingTeam = ChooseKickingTeam();
                clock.Advance(0);
            }
            else if (clockEvent == ClockEvent.TimeElapsed)
            {
                if (ShouldKickoff())
                {
                    PerformKickoff();
                }
            }
        }

        private string GetFullStatusMessage(string specifiedStatusMessage)
        {
            var awayTeamAbbreviation = AwayTeam.Abbreviation.PadLeft(3, ' ');
            var homeTeamAbbreviation = HomeTeam.Abbreviation.PadLeft(3, ' ');
            var awayTeamPossessionIndicator = NextPlay.Team == GameTeam.Away
                ? '•'
                : ' ';
            var homeTeamPossesionIndicator = NextPlay.Team == GameTeam.Home
                ? '•'
                : ' ';

            var scoreboard =
                $"{awayTeamAbbreviation} {awayTeamPossessionIndicator} {AwayScore}-{HomeScore} {homeTeamPossesionIndicator} {homeTeamAbbreviation}";

            string quarterDisplay = clock.PeriodNumber switch
            {
                >= 1 and <= 4 => $"Q{clock.PeriodNumber}",
                5 => "OT",
                > 5 => $"{clock.PeriodNumber - 4}OT",
                _ => throw new ArgumentOutOfRangeException(nameof(clock.PeriodNumber))
            };
            var minutes = clock.SecondsLeftInPeriod / 60;
            var seconds = clock.SecondsLeftInPeriod % 60;

            var timeDisplay = $"{quarterDisplay} {minutes:D2}:{seconds:D2}";

            var nextPlayDisplay = NextPlay.Kind switch
            {
                NextPlayKind.Kickoff => "Kickoff",
                NextPlayKind.FirstDown => "1st and",
                NextPlayKind.SecondDown => "2nd and",
                NextPlayKind.ThirdDown => "3rd and",
                NextPlayKind.FourthDown => "4th and",
                NextPlayKind.ConversionAttempt => "Conversion",
                NextPlayKind.FreeKick => "Free Kick",
                _ => throw new ArgumentOutOfRangeException()
            };

            var distanceDisplay = NextPlay.Kind is NextPlayKind.FirstDown or NextPlayKind.SecondDown
                or NextPlayKind.ThirdDown or NextPlayKind.FourthDown
                ? $" {GetDistanceToFirstDownLine(NextPlay)?.ToString() ?? "Goal"}"
                : "";

            var debugDecisionsString = DebugDecisionModeActive
                ? string.Join(Environment.NewLine, debugDecisions)
                : "";
            
            return string.Join(Environment.NewLine, specifiedStatusMessage, scoreboard,
                $"{timeDisplay} | {nextPlayDisplay}{distanceDisplay}", debugDecisionsString);
        }

        #region Coin Tosses and Kicking Teams

        private bool ShouldCoinTossForKickingTeam() =>
            clock.PeriodNumber is 1 or 3 or >= 5 && clock.SecondsLeftInPeriod == 0;

        private GameTeam ChooseKickingTeam()
        {
            // Home team, do you choose heads or tails?
            var homeTeamChoosesHeads = random.NextDouble() < 0.5d;
            AddDebugMessage($"{GetTeamAbbreviation(GameTeam.Home)} picks {(homeTeamChoosesHeads ? "heads" : "tails")}.");

            // The result of the coin toss is...
            var coinTossIsHeads = random.NextDouble() < 0.5d;
            AddDebugMessage($"The coin came up {(coinTossIsHeads ? "heads" : "tails")}.");

            if ((coinTossIsHeads && homeTeamChoosesHeads)
                || (!coinTossIsHeads && !homeTeamChoosesHeads))
            {
                // heads! Do you elect to receive or to kick?
                var homeChoice = HomeTeam.KickReturnStrength > AwayTeam.KickDefenseStrength
                    ? GameTeam.Home
                    : GameTeam.Away;
                
                AddDebugMessage($"{GetTeamAbbreviation(GameTeam.Home)} elects to {(homeChoice == GameTeam.Home ? "kick" : "receive")}.");

                return homeChoice;
            }

            // tails! Do you elect to receive or to kick?
            var awayChoice = AwayTeam.KickReturnStrength > HomeTeam.KickDefenseStrength
                ? GameTeam.Away
                : GameTeam.Home;

            AddDebugMessage(
                $"{GetTeamAbbreviation(GameTeam.Away)} elects to {(awayChoice == GameTeam.Away ? "kick" : "receive")}.");

            return awayChoice;
        }
        #endregion
        
        #region Kickoffs

        private void PerformKickoff()
        {
            var kickingTeam = GetTeamStrengths(NextPlay.Team);
            var receivingTeam = GetTeamStrengths(OtherTeam(NextPlay.Team));

            if (ShouldAttemptOnsideKick(NextPlay.Team))
            {
                var onsideStrengthDifferential = kickingTeam.KickingStrength - receivingTeam.KickDefenseStrength;
                AddDebugMessage($"The onside kick differential is {onsideStrengthDifferential:F2}.");
                
                var chanceOfRecoveryPercentage = 10d + (onsideStrengthDifferential / 50d);
                AddDebugMessage($"{GetTeamAbbreviation(NextPlay.Team)} has a {chanceOfRecoveryPercentage * 100:F2}% chance of recovering.");
                
                var kickRecoveredByKickingTeam = random.NextDouble() < chanceOfRecoveryPercentage / 100d;
                AddDebugMessage($"{GetTeamAbbreviation(NextPlay.Team)} has {(kickRecoveredByKickingTeam ? "recovered" : "not recovered")} the onside kick.");
                
                var kickDistanceTraveled = ClampDistanceBasedOnFieldPosition(TeamYardLineToInternalYardLine(35, NextPlay.Team),
                    SampleNormalDistribution(10d, onsideStrengthDifferential / 50d, random),
                    NextPlay.Direction,
                    TeamYardLineToInternalYardLine(1, OtherTeam(NextPlay.Team)),
                    TeamYardLineToInternalYardLine(99, NextPlay.Team));

                if (kickDistanceTraveled < 10d && kickRecoveredByKickingTeam) { kickDistanceTraveled = 10d; }

                NextPlay = NextPlayComputer.DetermineNextPlay(new PlayResult
                {
                    Kind = PlayResultKind.BallDead,
                    Team = kickRecoveredByKickingTeam ? NextPlay.Team : OtherTeam(NextPlay.Team),
                    DownNumber = null,
                    BallDeadYard = TeamYardLineToInternalYardLine(45, NextPlay.Team),
                    FirstDownLine = null,
                    Direction = TowardOpponentEndzone(NextPlay.Team)
                });
                
                AddDebugMessage($"Kick recovered at the {InternalYardNumberToString(NextPlay.LineOfScrimmage)}.");

                StatusMessage = GetFullStatusMessage(kickRecoveredByKickingTeam
                    ? $"Onside kick attempt recovered by {GetTeamAbbreviation(NextPlay.Team)}!"
                    : $"Onside kick attempt failed; ball recovered by {GetTeamAbbreviation(NextPlay.Team)}.");
            }
            else { }
        }
        
        private bool ShouldKickoff()
        {
            if (NextPlay.Kind == NextPlayKind.Kickoff)
            {
                return true;
            }

            if (clock.PeriodNumber is 1 or 3 or >= 5)
            {
                return clock.SecondsLeftInPeriod == 0;
            }

            return false;
        }

        private bool ShouldAttemptOnsideKick(GameTeam teamHomeOrAway)
        {
            var team = GetTeam(teamHomeOrAway);

            if (team.Disposition == TeamDisposition.Insane)
            {
                AddDebugMessage($"{team.Abbreviation} is insane, will attempt on onside kick.");
                return true;
            }

            if (clock.PeriodNumber < 4)
            {
                AddDebugMessage($"{team.Abbreviation} will not attempt an onside kick because it's the third quarter or earlier.");
                return false;
            }

            var scoreDifferential = GetCurrentScoreForTeam(teamHomeOrAway)
                - GetCurrentScoreForTeam(OtherTeam(teamHomeOrAway));

            var timeMargin = clock.PeriodNumber == 4
                ? 10 * 60
                : (2 * 60) + 30;

            var willAttemptOnsideKick = scoreDifferential < -8 && clock.SecondsLeftInPeriod < timeMargin;
            
            AddDebugMessage(
                $"{team.Abbreviation} {(willAttemptOnsideKick ? "will" : "will not")} attempt an onside kick because the score differential is {scoreDifferential} and there's {FormatSeconds(clock.SecondsLeftInPeriod)} seconds left in this period.");

            return willAttemptOnsideKick;
        }

        #endregion

        private static int? GetDistanceToFirstDownLine(NextPlay nextPlay) =>
            nextPlay.Direction == DriveDirection.TowardHomeEndzone
                ? nextPlay.LineOfScrimmage - nextPlay.FirstDownLine
                : nextPlay.FirstDownLine - nextPlay.LineOfScrimmage;

        private string GetTeamName(GameTeam team) =>
            team == GameTeam.Home
                ? currentGameRecord.HomeTeam.TeamName
                : currentGameRecord.AwayTeam.TeamName;

        private string GetTeamAbbreviation(GameTeam team) =>
            team == GameTeam.Home
                ? currentGameRecord.HomeTeam.Abbreviation
                : currentGameRecord.AwayTeam.Abbreviation;

        private Team GetTeam(GameTeam team) =>
            team == GameTeam.Home
                ? currentGameRecord.HomeTeam
                : currentGameRecord.AwayTeam;

        private InGameTeamStrengths GetTeamStrengths(GameTeam team) =>
            team == GameTeam.Home
                ? homeTeamStrengths
                : awayTeamStrengths;

        private int GetCurrentScoreForTeam(GameTeam team) =>
            currentGameRecord.QuarterBoxScores
                .Where(s => s.Team == team)
                .Sum(s => s.Score);

        private static double ClampDistanceBasedOnFieldPosition(double internalYardNumber,
            double distance,
            DriveDirection direction,
            double minInternalYardNumber = 0d,
            double maxInternalYardNumber = 100d)
        {
            return direction switch
            {
                DriveDirection.TowardHomeEndzone => Math.Max(internalYardNumber - distance, minInternalYardNumber),
                DriveDirection.TowardAwayEndzone => Math.Min(internalYardNumber + distance, maxInternalYardNumber),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private string InternalYardNumberToString(int yardNumber) =>
            yardNumber switch
            {
                50 => "midfield",
                < 50 => $"{GetTeamAbbreviation(GameTeam.Home)} {yardNumber}",
                _ => $"{GetTeamAbbreviation(GameTeam.Away)} {100 - yardNumber}"
            };

        private void AddDebugMessage(string message)
        {
            if (DebugDecisionModeActive)
            {
                debugDecisions.Add(message);
            }
        }

        private string GetNextPlayDebugMessage(NextPlay play)
        {
            var teamAbbreviation = GetTeamAbbreviation(play.Team);
            var lineOfScrimmageDisplay = InternalYardNumberToString(play.LineOfScrimmage);
            int? distanceToFirstDownLine = play.FirstDownLine.HasValue
                ? play.Direction == DriveDirection.TowardHomeEndzone
                    ? (play.LineOfScrimmage - play.FirstDownLine)
                    : (play.FirstDownLine - play.LineOfScrimmage)
                : null;

            return play.Kind switch
            {
                NextPlayKind.Kickoff => $"Next play is {teamAbbreviation} kickoff from {lineOfScrimmageDisplay}.",
                NextPlayKind.FirstDown => $"Next play is {teamAbbreviation} 1st and {distanceToFirstDownLine}.",
                NextPlayKind.SecondDown => $"Next play is {teamAbbreviation} 2nd and {distanceToFirstDownLine}.",
                NextPlayKind.ThirdDown => $"Next play is {teamAbbreviation} 3rd and {distanceToFirstDownLine}.",
                NextPlayKind.FourthDown => $"Next play is {teamAbbreviation} 4th and {distanceToFirstDownLine}.",
                NextPlayKind.ConversionAttempt =>
                    $"Next play is {DetermineArticle(teamAbbreviation)} {teamAbbreviation} conversion attempt.",
                NextPlayKind.FreeKick =>
                    $"Next play is {DetermineArticle(teamAbbreviation)} {teamAbbreviation} free kick.",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void MarkGameRecordComplete()
        {
            
        }
    }
}
