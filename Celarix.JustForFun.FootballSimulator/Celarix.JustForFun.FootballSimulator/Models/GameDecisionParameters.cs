using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed class GameDecisionParameters
    {
        private int awayTeamFourthDownTries = 0;
        private int homeTeamFourthDownTries = 0;
        private int awayTeamFourthDownConversions = 0;
        private int homeTeamFourthDownConversions = 0;

        public IRandom Random { get; init; }

        public Team AwayTeam { get; init; }
        public Team HomeTeam { get; init; }

        public TeamStrengthSet AwayTeamActualStrengths { get; set; }
        public TeamStrengthSet HomeTeamActualStrengths { get; set; }
        public TeamStrengthSet AwayTeamEstimateOfAway { get; set; }
        public TeamStrengthSet AwayTeamEstimateOfHome { get; set; }
        public TeamStrengthSet HomeTeamEstimateOfAway { get; set; }
        public TeamStrengthSet HomeTeamEstimateOfHome { get; set; }

        public GameType GameType { get; init; }

        public Team GetTeam(GameTeam team) =>
            team == GameTeam.Away ? AwayTeam : HomeTeam;

        public TeamDisposition GetDispositionForTeam(GameTeam team) =>
            team == GameTeam.Away ? AwayTeam.Disposition : HomeTeam.Disposition;

        public TeamStrengthSet GetActualStrengthsForTeam(GameTeam team) =>
            team == GameTeam.Away ? AwayTeamActualStrengths : HomeTeamActualStrengths;

        public TeamStrengthSet GetEstimateOfTeamByTeam(GameTeam estimatingTeam, GameTeam estimatedTeam)
        {
            return estimatingTeam switch
            {
                GameTeam.Away => estimatedTeam switch
                {
                    GameTeam.Away => AwayTeamEstimateOfAway,
                    GameTeam.Home => AwayTeamEstimateOfHome,
                    _ => throw new ArgumentOutOfRangeException(nameof(estimatedTeam), "Invalid GameTeam value")
                },
                GameTeam.Home => estimatedTeam switch
                {
                    GameTeam.Away => HomeTeamEstimateOfAway,
                    GameTeam.Home => HomeTeamEstimateOfHome,
                    _ => throw new ArgumentOutOfRangeException(nameof(estimatedTeam), "Invalid GameTeam value")
                },
                _ => throw new ArgumentOutOfRangeException(nameof(estimatingTeam), "Invalid GameTeam value")
            };
        }

        public double GetFourthDownConversionRate(GameTeam team)
        {
            if (team == GameTeam.Away)
            {
                return awayTeamFourthDownTries == 0
                    ? 0
                    : (double)awayTeamFourthDownConversions / awayTeamFourthDownTries;
            }
            else
            {
                return homeTeamFourthDownTries == 0
                    ? 0
                    : (double)homeTeamFourthDownConversions / homeTeamFourthDownTries;
            }
        }

        public void RecordFourthDownAttempt(GameTeam team, bool converted)
        {
            if (team == GameTeam.Away)
            {
                awayTeamFourthDownTries++;
                if (converted)
                {
                    awayTeamFourthDownConversions++;
                }
            }
            else
            {
                homeTeamFourthDownTries++;
                if (converted)
                {
                    homeTeamFourthDownConversions++;
                }
            }
        }
    }
}
