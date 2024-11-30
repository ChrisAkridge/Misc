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

                NextPlay = new NextPlay
                {
                    Direction = currentKickingTeam == GameTeam.Home
                        ? DriveDirection.TowardAwayEndzone
                        : DriveDirection.TowardHomeEndzone,
                    Kind = NextPlayKind.Kickoff,
                    LineOfScrimmage = TeamYardLineToInternalYardLine(35, currentKickingTeam),
                    Team = currentKickingTeam
                };
                
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

            StatusMessage =
                $"{GetTeamAbbreviation(GameTeam.Home)} chose {(homeTeamChoosesHeads ? "heads" : "tails")} and {(coinTossIsHeads ? "won" : "lost")}; {GetTeamAbbreviation(GameTeam.Away)} elects to {(awayChoice == GameTeam.Away ? "kick" : "receive")}.";

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
                AttemptOnsideKick(kickingTeam, receivingTeam);
            }
            else
            {
                Kickoff(kickingTeam, receivingTeam);
            }
        }

        private void AttemptOnsideKick(InGameTeamStrengths kickingTeam, InGameTeamStrengths receivingTeam)
        {
            var onsideStrengthDifferential = kickingTeam.KickingStrength - receivingTeam.KickDefenseStrength;
            AddDebugMessage($"The onside kick differential is {onsideStrengthDifferential:F2}.");

            var chanceOfRecoveryPercentage = 10d + (onsideStrengthDifferential / 50d);

            AddDebugMessage(
                $"{GetTeamAbbreviation(NextPlay.Team)} has a {chanceOfRecoveryPercentage:F2}% chance of recovering.");

            var kickRecoveredByKickingTeam = random.NextDouble() < chanceOfRecoveryPercentage / 100d;

            AddDebugMessage(
                $"{GetTeamAbbreviation(NextPlay.Team)} has {(kickRecoveredByKickingTeam ? "recovered" : "not recovered")} the onside kick.");

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
                BallDeadYard =
                    AddDistanceToYard(NextPlay.Direction, TeamYardLineToInternalYardLine(35, NextPlay.Team),
                        kickDistanceTraveled),
                FirstDownLine = null,
                Direction = TowardOpponentEndzone(NextPlay.Team)
            });

            AddDebugMessage($"Kick recovered at the {InternalYardNumberToString(NextPlay.LineOfScrimmage)}.");

            StatusMessage = GetFullStatusMessage(kickRecoveredByKickingTeam
                ? $"Onside kick attempt recovered by {GetTeamAbbreviation(NextPlay.Team)}!"
                : $"Onside kick attempt failed; ball recovered by {GetTeamAbbreviation(NextPlay.Team)}.");
        }

        private void Kickoff(InGameTeamStrengths kickingTeam, InGameTeamStrengths receivingTeam)
        {
            var kickDifferential = kickingTeam.KickingStrength - receivingTeam.KickDefenseStrength;
            var kickReturnDifferential = receivingTeam.KickReturnStrength - receivingTeam.RunningDefenseStrength;
            AddDebugMessage($"The kick differential is {kickDifferential:F2}; the kick return differential is {kickReturnDifferential:F2}.");

            var kickoffOutOfBoundsOdds = Math.Clamp(0.02d + (kickDifferential / 5000d), 0d, 1d);
            AddDebugMessage($"The kickoff has a {kickoffOutOfBoundsOdds * 100:F2}% chance of going out of bounds.");

            var kickoffOutOfBounds = random.NextDouble() < kickoffOutOfBoundsOdds;
            AddDebugMessage($"The kickoff {(kickoffOutOfBounds ? "went" : "did not go")} out of bounds.");

            var kickStrength = kickingTeam.KickingStrength - 1000d;

            // TODO: negative kick strengths should result in a reduction in mean with no stddev change
            // positive kick strengths result in an increase in stddev with no mean change
            // i guess
            // var kickDistance = SampleNormalDistribution(65d, 3d * (kickStrength / 50d), random);
            var kickDistance = SampleNormalDistribution(new NormalDistributionParameters(65d, 3d, 0.1d, 0.06d), kickStrength, random);

            var kickLandingYard = AddDistanceToYard(NextPlay.Direction, NextPlay.LineOfScrimmage, kickDistance);
            AddDebugMessage($"The kickoff traveled {kickDistance:F2} yards.");

            if (kickLandingYard < -10d || kickLandingYard >= 110d)
            {
                NextPlay = NextPlayComputer.DetermineNextPlay(new PlayResult
                {
                    BallDeadYard = null,
                    Direction = TowardOpponentEndzone(OtherTeam(NextPlay.Team)),
                    DownNumber = null,
                    Kind = PlayResultKind.BallDead,
                    Team = OtherTeam(NextPlay.Team)
                });
                
                // TODO: better penalty handling
                if (kickoffOutOfBounds)
                {
                    NextPlay.LineOfScrimmage = AddDistanceToYard(NextPlay.Direction, NextPlay.LineOfScrimmage, 5d);
                    NextPlay.FirstDownLine = AddDistanceToYard(NextPlay.Direction, NextPlay.FirstDownLine!.Value, 5d);
                }

                StatusMessage = GetFullStatusMessage($"Touchback for {GetTeamAbbreviation(NextPlay.Team)}.");

                return;
            }

            var receivingTeamSignalsFairCatch = kickReturnDifferential < -500d;
            AddDebugMessage($"{GetTeamAbbreviation(OtherTeam(NextPlay.Team))} {(receivingTeamSignalsFairCatch ? "has signaled fair catch" : "to return")}.");

            //if (!receivingTeamSignalsFairCatch)
            //{
            var ballCaughtByReceivingTeamOdds =
                0.9999d - (0.0001d * ((receivingTeam.KickReturnStrength - 1000d) / 50d));
            AddDebugMessage($"{GetTeamAbbreviation(OtherTeam(NextPlay.Team))} has a {ballCaughtByReceivingTeamOdds * 100d:F2}% chance of catching the ball.");

            var ballCaughtByReceivingTeam = random.NextDouble() < ballCaughtByReceivingTeamOdds;
            AddDebugMessage(
                $"{GetTeamAbbreviation(OtherTeam(NextPlay.Team))} has {(ballCaughtByReceivingTeam ? "caught" : "not caught")} the kickoff.");

            if (ballCaughtByReceivingTeam)
            {
                NextPlay = NextPlayComputer.DetermineNextPlay(new PlayResult
                {
                    Kind = PlayResultKind.BallDead,
                    Team = OtherTeam(NextPlay.Team),
                    DownNumber = null,
                    BallDeadYard = kickLandingYard,
                    FirstDownLine = null,
                    Direction = TowardOpponentEndzone(OtherTeam(NextPlay.Team))
                });

                StatusMessage = GetFullStatusMessage(
                        $"{GetTeamAbbreviation(NextPlay.Team)} signals fair catch and caught the ball at {InternalYardNumberToString(NextPlay.LineOfScrimmage)}.");

                return;
            }
            else
            {
                // Handle fumble
            }
            
            //}
            //else
            //{
            //    // TODO: better penalty handling

            //}
        }

        private bool ShouldKickoff()
        {
            if (clock.PeriodNumber is 1 or 3 or >= 5)
            {
                return clock.SecondsElapsedInPeriod == 0;
            }
            
            if (NextPlay.Kind == NextPlayKind.Kickoff)
            {
                return true;
            }

            return false;
        }

        private bool ShouldAttemptOnsideKick(GameTeam teamHomeOrAway)
        {
            var team = GetTeam(teamHomeOrAway);

            if (team.Disposition == TeamDisposition.Insane)
            {
                AddDebugMessage($"{team.Abbreviation} is insane, will attempt an onside kick.");
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
                ? nextPlay.FirstDownLine - nextPlay.LineOfScrimmage
                : nextPlay.LineOfScrimmage - nextPlay.FirstDownLine;

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
            switch (direction)
            {
                case DriveDirection.TowardAwayEndzone:
                {
                    var distanceToMinYard = internalYardNumber - minInternalYardNumber;

                    return Math.Min(distanceToMinYard, distance);
                }
                case DriveDirection.TowardHomeEndzone:
                {
                    var distanceToMaxYard = maxInternalYardNumber - internalYardNumber;

                    return Math.Min(distanceToMaxYard, distance);
                }
                default: throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        private string InternalYardNumberToString(int yardNumber) =>
            yardNumber switch
            {
                50 => "midfield",
                < 50 => $"{GetTeamAbbreviation(GameTeam.Away)} {yardNumber}",
                _ => $"{GetTeamAbbreviation(GameTeam.Home)} {100 - yardNumber}"
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

        private int AddDistanceToYard(DriveDirection direction, int internalYardNumber, double distance)
        {
            return direction switch
            {
                DriveDirection.TowardHomeEndzone => internalYardNumber
                    + (int)Math.Round(distance, MidpointRounding.ToZero),
                DriveDirection.TowardAwayEndzone => internalYardNumber
                    - (int)Math.Round(distance, MidpointRounding.ToPositiveInfinity),
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
        }

        public void MarkGameRecordComplete()
        {
            
        }
    }
}
