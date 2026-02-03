using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class StartNextPeriodStep
    {
        public static GameContext Run(GameContext context)
        {
            var playContext = context.Environment.CurrentPlayContext!;
            var nextQuarterActions = new NextQuarterActions(playContext.PeriodNumber);

            if (nextQuarterActions.CoinTossNeeded)
            {
                var random = context.Environment.RandomFactory.Create();
                var awayReceives = random.NextDouble() < 0.5;
                var kickingTeam = awayReceives ? GameTeam.Home : GameTeam.Away;
                playContext = playContext with
                {
                    CoinFlipWinner = awayReceives ? GameTeam.Away : GameTeam.Home,
                    NextPlay = NextPlayKind.Kickoff,
                    // Remember, the other team kicks off
                    TeamWithPossession = kickingTeam,
                    LineOfScrimmage = playContext.TeamYardToInternalYard(kickingTeam, 35),
                    LineToGain = null,
                    AwayTimeoutsRemaining = nextQuarterActions.NextPeriodNumber <= 4 ? 3 : 2,
                    HomeTimeoutsRemaining = nextQuarterActions.NextPeriodNumber <= 4 ? 3 : 2
                };
            }
            else if (nextQuarterActions.CoinTossLoserReceivesPossession)
            {
                var kickingTeam = playContext.CoinFlipWinner;
                playContext = playContext with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    // Remember, the other team kicks off
                    TeamWithPossession = playContext.CoinFlipWinner,
                    LineOfScrimmage = playContext.TeamYardToInternalYard(kickingTeam, 35),
                    LineToGain = null
                };
            }

            context.Environment.CurrentPlayContext = playContext with
            {
                PeriodNumber = playContext.PeriodNumber + 1,
                SecondsLeftInPeriod = playContext.PeriodNumber > 4
                    ? Constants.SecondsPerOvertimePeriod
                    : Constants.SecondsPerQuarter
            };

            Helpers.SaveQuarterBoxScores(context);
            context.Environment.FootballRepository!.SaveChanges();

            Log.Information("StartNextPeriodStep: Started period {PeriodNumber}.", context.Environment.CurrentPlayContext.PeriodNumber);
            return context.WithNextState(GameState.EvaluatingPlay);
        }

        
    }
}
