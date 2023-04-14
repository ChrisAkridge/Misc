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
            var awayTeamPossessionIndicator = nextPlay.Team == GameTeam.Away
                ? '•'
                : ' ';
            var homeTeamPossesionIndicator = nextPlay.Team == GameTeam.Home
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

            var nextPlayDisplay = nextPlay.Kind switch
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

            var distanceDisplay = nextPlay.Kind is NextPlayKind.FirstDown or NextPlayKind.SecondDown
                or NextPlayKind.ThirdDown or NextPlayKind.FourthDown
                ? $" {GetDistanceToFirstDownLine(nextPlay)?.ToString() ?? "Goal"}"
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
            var kickingTeam = GetTeamStrengths(nextPlay.Team);
            var receivingTeam = GetTeamStrengths(OtherTeam(nextPlay.Team));

            if (ShouldAttemptOnsideKick(nextPlay.Team))
            {
                var onsideStrengthDifferential = kickingTeam.KickingStrength - receivingTeam.KickDefenseStrength;
                var chanceOfRecoveryPercentage = 10d + (onsideStrengthDifferential / 50d);
                var kickRecoveredByKickingTeam = random.NextDouble() < chanceOfRecoveryPercentage / 100d;
                // WYLO: ughhhh. also, the clamp should be only to the defense's 1-yard line.
                // also ughhhhhhh.
                var kickDistanceTraveled = ClampDistanceBasedOnFieldPosition(TeamYardLineToInternalYardLine(35, nextPlay.Team),
                    SampleNormalDistribution(10d, onsideStrengthDifferential / 50d, random), nextPlay.Direction);

                if (kickDistanceTraveled < 10d && kickRecoveredByKickingTeam) { kickDistanceTraveled = 10d; }

                nextPlay = NextPlayComputer.DetermineNextPlay(new PlayResult
                {
                    Kind = PlayResultKind.BallDead,
                    Team = kickRecoveredByKickingTeam ? nextPlay.Team : OtherTeam(nextPlay.Team),
                    DownNumber = null,
                    BallDeadYard = TeamYardLineToInternalYardLine(45, nextPlay.Team),
                    FirstDownLine = null,
                    Direction = TowardOpponentEndzone(nextPlay.Team)
                });
                
                
            }
            else { }
        }
        
        private bool ShouldKickoff()
        {
            if (nextPlay.Kind == NextPlayKind.Kickoff)
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

        private double ClampDistanceBasedOnFieldPosition(double internalYardNumber, double distance, DriveDirection direction)
        {
            return double.NaN;
        }

        private void AddDebugMessage(string message)
        {
            if (DebugDecisionModeActive)
            {
                debugDecisions.Add(message);
            }
        }

        public void MarkGameRecordComplete()
        {
            
        }
    }
}
