using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class PostPlayCheck
    {
        public static GameContext Run(GameContext context)
        {
            var decision = PostPlayDecision.RunNextPlay;
            var playContext = context.Environment.CurrentPlayContext!;
            var nextQuarterActions = new NextQuarterActions(playContext.PeriodNumber);

            if (playContext.PeriodNumber >= 5)
            {
                if (playContext.AwayScoredThisPlay || playContext.HomeScoredThisPlay)
                {
                    var bothTeamsHadPossession = context.Environment.CurrentGameRecord!.TeamDriveRecords
                    .Where(r => r.QuarterNumber >= 5)
                    .Select(r => r.TeamID)
                    .Distinct()
                    .Count() == 2;
                    if (bothTeamsHadPossession && playContext.HomeScore != playContext.AwayScore)
                    {
                        decision = PostPlayDecision.EndGameWin;
                    }
                }
            }
            else if (playContext.SecondsLeftInPeriod == 0)
            {
                if (playContext.PeriodNumber >= 4 && playContext.HomeScore != playContext.AwayScore)
                {
                    decision = PostPlayDecision.EndGameWin;
                }

                var gameIsRegularSeason = context.Environment.CurrentGameRecord!.GameType == GameType.RegularSeason;
                if (playContext.PeriodNumber == 5 && gameIsRegularSeason && playContext.HomeScore == playContext.AwayScore)
                {
                    decision = PostPlayDecision.EndGameTie;
                }
                else
                {
                    decision = PostPlayDecision.NextPeriod;
                }
            }
            else if (context.TeamWithPossession != playContext.TeamWithPossession)
            {
                decision = PostPlayDecision.EndOfPossession;
            }

            bool saveDriveRecord = decision is PostPlayDecision.EndGameWin
                or PostPlayDecision.EndGameTie
                or PostPlayDecision.EndOfPossession;
            if (decision == PostPlayDecision.NextPeriod
                && playContext.SecondsLeftInPeriod == 0
                && nextQuarterActions.CurrentDriveEnds)
            {
                saveDriveRecord = true;
            }
            if (saveDriveRecord)
            {
                SaveDriveRecord(context);
            }

            Log.Information("PostPlayCheck: Decision = {Decision}", decision);
            return context.WithNextState(decision switch
            {
                PostPlayDecision.RunNextPlay => GameState.EvaluatingPlay,
                PostPlayDecision.EndOfPossession => GameState.EvaluatingPlay,
                PostPlayDecision.NextPeriod => GameState.StartNextPeriod,
                PostPlayDecision.EndGameWin => GameState.EndGame,
                PostPlayDecision.EndGameTie => GameState.EndGame,
                _ => throw new InvalidOperationException("Unhandled PostPlayDecision value."),
            });
        }

        internal static void SaveDriveRecord(GameContext context)
        {
            var playContext = context.Environment.CurrentPlayContext!;
            var gameRecord = context.Environment.CurrentGameRecord!;
            var driveTeamID = context.TeamWithPossession == GameTeam.Away
                ? gameRecord.AwayTeamID
                : gameRecord.HomeTeamID;
            var driveRecord = playContext.BuildTeamDriveRecord(gameRecord.GameID,
                driveTeamID, context.PlayCountOnDrive);
            context.Environment.FootballRepository.AddTeamDriveRecord(driveRecord);
            context.Environment.FootballRepository.SaveChanges();
        }
    }
}
