using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using static Celarix.JustForFun.FootballSimulator.Helpers;

namespace Celarix.JustForFun.FootballSimulator
{
    [Obsolete("See Gameplay.FootballGame instead for the main implementation.")]
    internal sealed class GameLoop
    {
        private const int SecondsInRegulationQuarter = 15 * 60;

        private readonly FootballContext context;
        private readonly GameRecord currentGame;
        private readonly Random random;
        private readonly int secondsInOvertimePeriod;

        private InGameTeamStrengths homeTeamStrengths;
        private InGameTeamStrengths awayTeamStrengths;

        private int quarterNumber;
        private int secondsLeftInQuarter;
        private GameTeam firstHalfKickingTeam;
        private NextPlay nextPlay;

        private Team HomeTeam => currentGame.HomeTeam;
        private Team AwayTeam => currentGame.AwayTeam;

        private int AwayScore =>
            currentGame
                .QuarterBoxScores
                .Where(q => q.Team == GameTeam.Away)
                .Sum(q => q.Score);

        private int HomeScore =>
            currentGame
                .QuarterBoxScores
                .Where(q => q.Team == GameTeam.Home)
                .Sum(q => q.Score);

        public string StatusMessage { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public GameLoop(FootballContext context, GameRecord currentGame)
        {
            this.context = context;
            this.currentGame = currentGame;
            random = new Random();
            
            homeTeamStrengths = InGameTeamStrengths.FromTeam(currentGame.HomeTeam);
            awayTeamStrengths = InGameTeamStrengths.FromTeam(currentGame.AwayTeam);

            quarterNumber = 1;
            secondsLeftInQuarter = SecondsInRegulationQuarter;

            secondsInOvertimePeriod = currentGame.WeekNumber > 17
                ? 15 * 60
                : 10 * 60;

            DetermineFirstHalfKickingTeam();
        }

        public void RunNextAction()
        {
            if (quarterNumber == 1 && secondsLeftInQuarter == SecondsInRegulationQuarter)
            {
                // Ready for kickoff!
                nextPlay = new NextPlay
                {
                    Kind = NextPlayKind.Kickoff,
                    Team = firstHalfKickingTeam,
                    LineOfScrimmage = 35,
                    FirstDownLine = null,
                    Direction = TowardOpponentEndzone(firstHalfKickingTeam)
                };

                StatusMessage = $"{GetTeamName(nextPlay.Team)} to kickoff!";
            }

            if (nextPlay.Kind == NextPlayKind.Kickoff)
            {
                
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

            string quarterDisplay = quarterNumber switch
            {
                >= 1 and <= 4 => $"Q{quarterNumber}",
                5 => "OT",
                > 5 => $"{quarterNumber - 4}OT",
                _ => throw new ArgumentOutOfRangeException(nameof(quarterNumber))
            };
            var minutes = secondsLeftInQuarter / 60;
            var seconds = secondsLeftInQuarter % 60;

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

            return string.Join(Environment.NewLine, specifiedStatusMessage, scoreboard,
                $"{timeDisplay} | {nextPlayDisplay}{distanceDisplay}");
        }

        private void DetermineFirstHalfKickingTeam()
        {
            // Home team, do you choose heads or tails?
            var homeTeamChoosesHeads = random.NextDouble() < 0.5d;
            
            // The result of the coin toss is...
            var coinTossIsHeads = random.NextDouble() < 0.5d;

            if ((coinTossIsHeads && homeTeamChoosesHeads)
                || (!coinTossIsHeads && !homeTeamChoosesHeads))
            {
                // heads! Do you elect to receive or to kick?
                firstHalfKickingTeam = HomeTeam.KickReturnStrength > AwayTeam.KickDefenseStrength
                    ? GameTeam.Home
                    : GameTeam.Away;
            }
            else
            {
                // tails! Do you elect to receive or to kick?
                firstHalfKickingTeam = AwayTeam.KickReturnStrength > HomeTeam.KickDefenseStrength
                    ? GameTeam.Away
                    : GameTeam.Home;
            }
        }

        #region Kickoff
        private void DetermineKickoffResult()
        {
            // The kicking team will attempt an onside kick recovery if:
            //  - The team's disposition is Insane, or
            //  - There are under 10 minutes in the game and the team is down by more than 8 points.
            var kickingTeam = GetTeam(nextPlay.Team);
            var receivingTeam = GetTeam(OtherTeam(nextPlay.Team));

            if (ShouldAttemptOnsideKick(kickingTeam, nextPlay.Team))
            {
                // Balance onside kick success chance such that equal onside kick strength and onside kick defense strength
                // results in a 10% chance of recovery for the kicking team. Each 50 points of differential adds 1% to these
                // odds, capped at 100%.
                var onsideStrengthDifferential = kickingTeam.KickingStrength - receivingTeam.KickDefenseStrength;
                var chanceOfRecoveryPercentage = 10d + (onsideStrengthDifferential / 50d);
                var kickRecoveredByKickingTeam = random.NextDouble() < chanceOfRecoveryPercentage / 100d;
                var elapsedSeconds = (int)(2d + (random.NextDouble() * 2d));

                nextPlay = NextPlayComputer.DetermineNextPlay(new PlayResult
                {
                    Kind = PlayResultKind.BallDead,
                    Team = kickRecoveredByKickingTeam ? nextPlay.Team : OtherTeam(nextPlay.Team),
                    DownNumber = null,
                    BallDeadYard = TeamYardLineToInternalYardLine(45, nextPlay.Team),
                    FirstDownLine = null,
                    Direction = TowardOpponentEndzone(nextPlay.Team)
                });

                AdjustStrengthByBooleanEvent(nextPlay.Team, s => s.KickingStrength, (s, d) => s.KickingStrength = d,
                    chanceOfRecoveryPercentage, kickRecoveredByKickingTeam);
                AdjustStrengthByBooleanEvent(OtherTeam(nextPlay.Team), s => s.KickDefenseStrength, (s, d) => s.KickDefenseStrength = d,
                    chanceOfRecoveryPercentage, !kickRecoveredByKickingTeam);
                
                var clockEvent = RemoveTimeFromClock(elapsedSeconds);
                SetNextPlayOrHandleClockEvent(nextPlay, clockEvent);
            }
            else
            {
                
            }
        }

        private bool ShouldAttemptOnsideKick(Team team, GameTeam teamHomeOrAway)
        {
            if (team.Disposition == TeamDisposition.Insane) { return true; }

            if (quarterNumber < 4) { return false; }

            var scoreDifferential = GetCurrentScoreForTeam(teamHomeOrAway)
                - GetCurrentScoreForTeam(OtherTeam(teamHomeOrAway));
            var timeMargin = quarterNumber == 4
                ? 10 * 60
                : (2 * 60) + 30;

            return scoreDifferential < -8 && secondsLeftInQuarter < timeMargin;
        }
        
        #endregion
        private static int? GetDistanceToFirstDownLine(NextPlay nextPlay)
        {
            if (nextPlay.Direction == DriveDirection.TowardHomeEndzone)
            {
                return nextPlay.LineOfScrimmage - nextPlay.FirstDownLine;
            }

            return nextPlay.FirstDownLine - nextPlay.LineOfScrimmage;
        }

        private string GetTeamName(GameTeam team) =>
            team == GameTeam.Home
                ? currentGame.HomeTeam.TeamName
                : currentGame.AwayTeam.TeamName;

        private Team GetTeam(GameTeam team) =>
            team == GameTeam.Home
                ? currentGame.HomeTeam
                : currentGame.AwayTeam;

        private int GetCurrentScoreForTeam(GameTeam team) =>
            currentGame.QuarterBoxScores
                .Where(s => s.Team == team)
                .Sum(s => s.Score);

        private ClockEvent RemoveTimeFromClock(int seconds)
        {
            // TODO: no overtime in preseason, only 1 overtime in regular season
            
            secondsLeftInQuarter -= seconds;

            if (secondsLeftInQuarter < 0)
            {
                if (quarterNumber is 1 or 3)
                {
                    secondsLeftInQuarter = SecondsInRegulationQuarter;
                    quarterNumber += 1;

                    return ClockEvent.EndOfQuarter;
                }

                if (quarterNumber == 2)
                {
                    secondsLeftInQuarter = SecondsInRegulationQuarter;
                    quarterNumber = 3;

                    return ClockEvent.EndOfHalf;
                }

                if (quarterNumber >= 4
                    && GetCurrentScoreForTeam(GameTeam.Home) == GetCurrentScoreForTeam(GameTeam.Away))
                {
                    secondsLeftInQuarter = secondsInOvertimePeriod;
                    quarterNumber = 5;

                    return ClockEvent.StartingOvertimePeriod;
                }

                if (quarterNumber >= 4) { return ClockEvent.EndOfGame; }
            }

            return ClockEvent.TimeElapsed;
        }

        private void SetNextPlayOrHandleClockEvent(NextPlay nextPlay, ClockEvent clockEvent)
        {
            switch (clockEvent)
            {
                case ClockEvent.TimeElapsed:
                    this.nextPlay = nextPlay;

                    break;
                case ClockEvent.EndOfQuarter:
                    this.nextPlay = nextPlay;
                    StatusMessage += $"{Environment.NewLine} End of {GetQuarterOrdinal()}.";
                    break;
                case ClockEvent.EndOfHalf:
                    var secondHalfKickingTeam = OtherTeam(firstHalfKickingTeam);

                    this.nextPlay = new NextPlay
                    {
                        Kind = NextPlayKind.Kickoff,
                        Team = secondHalfKickingTeam,
                        LineOfScrimmage = TeamYardLineToInternalYardLine(35, secondHalfKickingTeam),
                        FirstDownLine = null,
                        Direction = TowardOpponentEndzone(secondHalfKickingTeam)
                    };

                    StatusMessage +=
                        $"{Environment.NewLine}End of first half. {GetTeamName(secondHalfKickingTeam)} to kickoff!";
                    break;
                case ClockEvent.EndOfGame:
                    StatusMessage += $"{Environment.NewLine}Game is over.";
                    break;
                case ClockEvent.StartingOvertimePeriod:
                    // TODO: how does an NFL overtime period start? is there a kickoff?
                    
                    StatusMessage = $"{Environment.NewLine}We're going into {GetQuarterOrdinal()}!";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clockEvent), clockEvent, null);
            }
        }

        private void AdjustStrengthByBooleanEvent(GameTeam gameTeam,
            Func<InGameTeamStrengths, double> propertyGetter,
            Action<InGameTeamStrengths, double> propertySetter,
            double oddsOfEvent,
            bool eventOccurred)
        {
            // The less likely the event, the more strength we want to add. The basic formula is +2 points for making an
            // event with a 10% chance, and this value is multiplied by 4 for every order of magnitude less than 10% (so
            // making a 1% chance event would be worth 2 * 4 = 8 points, a 0.1% chance event 2 * 4 * 4 = 32 points). Making
            // a 100% chance event is worth nothing.
            
            // If you miss the event, you lose more strength the more likely it was. We take 1 - the probability and do the
            // same 2 * 4^n calculation, but this time subtracting it from the strength. So, if you miss an event with 90%
            // probability, you lose 2 points of strength. Missing a 99% chance event loses you 8, and so forth.

            double changeInScore;
            
            if (eventOccurred)
            {
                var belsProbability = Math.Log10(oddsOfEvent);
                changeInScore = 2 * Math.Pow(4d, -belsProbability - 1d);
            }
            else
            {
                var belsProbabilityAgainst = Math.Log10(1d - oddsOfEvent);
                changeInScore = -(2 * Math.Pow(4d, -belsProbabilityAgainst - 1d));
            }

            var teamStrengths = gameTeam == GameTeam.Home
                ? homeTeamStrengths
                : awayTeamStrengths;
            var currentScore = propertyGetter(teamStrengths);
            propertySetter(teamStrengths, currentScore + changeInScore);
        }

        private string GetQuarterOrdinal() =>
            quarterNumber switch
            {
                1 => "1st quarter",
                2 => "2nd quarter",
                3 => "3rd quarter",
                4 => "4th quarter",
                5 => "OT",
                >= 6 => $"{quarterNumber - 4}OT",
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}
