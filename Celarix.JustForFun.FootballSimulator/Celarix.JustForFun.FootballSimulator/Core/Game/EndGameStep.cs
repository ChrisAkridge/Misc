using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class EndGameStep
    {
        public static GameContext Run(GameContext context)
        {
            Helpers.SaveQuarterBoxScores(context);
            var gameRecord = context.Environment.CurrentGameRecord!;
            var repository = context.Environment.FootballRepository;
            repository.CompleteGame(gameRecord.GameID);

            var awayTeam = gameRecord.AwayTeam;
            var homeTeam = gameRecord.HomeTeam;

            // By now, the strengths of these teams are stored in these properties
            // on the GameRecord, so we can just copy them over to the teams.
            repository.SetTeamStrengths(awayTeam, awayTeam.TeamID);
            repository.SetTeamStrengths(homeTeam, homeTeam.TeamID);
            repository.SaveChanges();

            // This state points to itself and signals GameCompleted to the system state machine.
            return context.WithNextState(GameState.EndGame);
        }
    }
}
