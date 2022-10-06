using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.FootballSimulatorLite
{
    public sealed class FootballGame
    {
        private Random random;
        
        private FootballTeam homeTeam;
        private FootballTeam awayTeam;
        private bool homeTeamReceivesOpeningKickoff;
        private Possession possession;
        private int homeScore;
        private int awayScore;
        private double lineOfScrimmage;
        private double? lineToGain;

        private int quarter;
        private int secondsLeftInQuarter;
        private PlayType nextPlayType;
        
        public string LastPlayDescription { get; private set; }
        
        public int Year { get; }
        public int Week { get; }
        public int GameNumber { get; }
        
        public bool GameInProgress { get; private set; }

        public FootballGame(FootballTeam homeTeam, FootballTeam awayTeam, int year, int week, int gameNumber)
        {
            random = new Random();
            this.homeTeam = homeTeam;
            this.awayTeam = awayTeam;
            Year = year;
            Week = week;
            GameNumber = gameNumber;

            var coinToss = random.NextDouble();
            var coinTossWinner = coinToss < 0.5d
                ? homeTeam
                : awayTeam;
            var winnerElectsToKick = random.NextDouble() < 0.5d;
            switch (winnerElectsToKick)
            {
                case true when coinTossWinner == awayTeam:
                case false when coinTossWinner == homeTeam:
                    homeTeamReceivesOpeningKickoff = true;
                    break;
            }

            // Set up opening kickoff - kicking team gets possession for just the
            // kickoff.
            possession = homeTeamReceivesOpeningKickoff
                ? Possession.Away
                : Possession.Home;
            lineOfScrimmage = homeTeamReceivesOpeningKickoff
                ? 65d   // Away's 35-yard line
                : 35d;
            quarter = 1;
            secondsLeftInQuarter = 15 * 60;
            nextPlayType = PlayType.Kickoff;

            LastPlayDescription = GetDescriptionFirstLine()
                + $"{(possession == Possession.Home ? homeTeam.Location : awayTeam.Location)} to kickoff.";
            
            GameInProgress = true;
        }

        public void ExecuteNextPlay()
        {
            var us = possession == Possession.Home
                ? homeTeam
                : awayTeam;
            var them = us == homeTeam
                ? awayTeam
                : homeTeam;

            switch (nextPlayType)
            {
                case PlayType.Kickoff:
                    ExecuteKickoff(us, them);
                    break;
            }
        }

        private void ExecuteKickoff(FootballTeam us, FootballTeam them)
        {
            // Decision 1: onside kick or normal kick?
            if (quarter >= 4
                && secondsLeftInQuarter < 300
                && Score(us) < Score(them)
                && random.NextDouble() < 0.99d)
            {
                ExecuteOnsideKick(us, them);
            }
            
            // No, just do a normal kickoff. You need a strength of at least 1000
            // to be able to do touchbacks.
            var maxPossibleDistance = 0.0075d * us.KickoffStrength;
            var actualDistance = random.NextDouble() * maxPossibleDistance;
            var touchback = actualDistance >= 75d;
            var kickoffWentOutOfBounds = !touchback && ChooseWeightedOption(us.KickoffStrength, 250) == 0;
            var theySignalFairCatch = us.RushDefenseStrength > them.KickReturnStrength * 1.15d;

            if (kickoffWentOutOfBounds)
            {
                var outOfBoundsLocation = random.NextDouble() * 75d;
                var theyTake30YardOption = IsBehind(them, outOfBoundsLocation, Forward(us, 30d));
                ChangePossesion();
                nextPlayType = PlayType.First;
                lineOfScrimmage = theyTake30YardOption
                    ? Forward(us, 30d)
                    : outOfBoundsLocation;
                lineToGain = Forward(them, 10d);
            }
            else if (theySignalFairCatch)
            {
                if (Forward(us, actualDistance) < 0d)
                {
                    ChangePossesion();
                    nextPlayType = PlayType.First;
                    lineOfScrimmage = possession == Possession.Home
                        ? 25d
                        : 75d;
                    lineToGain = Forward(them, 10d);
                }
                else
                {
                    ChangePossesion();
                    nextPlayType = PlayType.First;
                    lineOfScrimmage = Forward(us, actualDistance);
                    // TODO: make lineToGain a property that lets you assign
                    // distances in the endzone that become Goal-To-Go automatically
                    lineToGain = Forward(them, 10d);
                }
            }
            else
            {
                // Return the kick!
            }
        }

        private void ExecuteOnsideKick(FootballTeam us, FootballTeam them)
        {
            var onsideKickStrength = us.KickoffStrength * 0.5d;
            var onsideKickSuccessful = ChooseWeightedOption(onsideKickStrength, them.KickReturnStrength) == 0;

            if (onsideKickSuccessful)
            {
                // Gain 12.5-25 kickoff points!
                us.KickoffStrength += 12.5d + (random.NextDouble() * 12.5d);
                // And the ball!
                nextPlayType = PlayType.First;
                lineOfScrimmage = Forward(us, 10d);
                lineToGain = Forward(us, 10d);
            }
            else
            {
                us.KickoffStrength -= (7.5d + (random.NextDouble() * 12.5d)).ClampLow(10d);
                ChangePossesion();
                nextPlayType = PlayType.First;
                lineOfScrimmage = Forward(us, 10d);
                lineToGain = Forward(them, 10d);
            }
        }
        
        private int Score(FootballTeam team) => team == homeTeam
            ? homeScore
            : awayScore;

        private int ChooseWeightedOption(params double[] strengths)
        {
            var sum = strengths.Sum();
            var runningTotal = 0d;
            var randomNumber = random.NextDouble() * sum;

            for (int i = 0; i < strengths.Length; i++)
            {
                runningTotal += strengths[i];
                if (randomNumber < runningTotal) { return i; }
            }

            return strengths.Length - 1;
        }

        private string GetDescriptionFirstLine()
        {
            var gameNumber = $"{Year} W{Week}G{GameNumber}";
            var awayInfo = $"{(possession == Possession.Away ? " •" : "  ")}{awayTeam.Abbreviation}";
            var homeInfo = $"{homeTeam.Abbreviation}{(possession == Possession.Home ? "• " : "  ")}";
            var scoreInfo = $"{awayScore.ToString(),-3}-{homeScore,3}";
            
            var distanceToFirstDown = (lineToGain != null)
                ? (possession == Possession.Home)
                    ? (lineToGain - lineOfScrimmage).ToString()
                    : (lineOfScrimmage - lineToGain).ToString()
                : "Goal";
            var downInfo = nextPlayType switch
            {
                PlayType.First => $"1st and {distanceToFirstDown}",
                PlayType.Second => $"2nd and {distanceToFirstDown}",
                PlayType.Third => $"3rd and {distanceToFirstDown}",
                PlayType.Fourth => $"4th and {distanceToFirstDown}",
                PlayType.Kickoff => "Kickoff",
                PlayType.FieldGoalAttempt => "FG Att.",
                PlayType.FreeKick => "Free Kick",
                PlayType.ExtraPointAttempt => "XP Att.",
                PlayType.TwoPointConversionAttempt => "2pt. Att.",
                _ => throw new ArgumentOutOfRangeException()
            };
            var lineOfScrimmageLocation = $"Ball on {GetYardMarker(lineOfScrimmage)}";

            return
                $"{gameNumber} {awayInfo}{scoreInfo}{homeInfo} | {downInfo} | {lineOfScrimmageLocation}{Environment.NewLine}";
        }

        private string GetYardMarker(double yard) =>
            yard switch
            {
                >= -10d and < 0d => $"{-yard:F0} yard(s) deep in {homeTeam.Abbreviation} endzone",
                >= 0d and < 50d => $"{homeTeam.Abbreviation} {yard:F0}",
                >= 50d and < 100d => $"{awayTeam.Abbreviation} {100d - yard:F0}",
                > 100d and < 110d => $"{yard - 100:F0} yard(s) deep in {awayTeam.Abbreviation} endzone",
                _ => throw new ArgumentOutOfRangeException(nameof(yard))
            };

        private double Forward(FootballTeam team, double yards) =>
            team == homeTeam
                ? lineOfScrimmage + yards
                : lineOfScrimmage - yards;

        private void ChangePossesion() =>
            possession = (possession == Possession.Home)
                ? Possession.Away
                : Possession.Home;

        private bool IsBehind(FootballTeam fromPerspectiveOf, double a, double b) =>
            fromPerspectiveOf == homeTeam
                ? b >= a
                : b <= a;
    }
}
