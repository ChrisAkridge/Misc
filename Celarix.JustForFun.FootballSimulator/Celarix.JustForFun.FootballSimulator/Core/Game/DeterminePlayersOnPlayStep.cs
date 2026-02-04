using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class DeterminePlayersOnPlayStep
    {
        public static GameContext Run(GameContext context)
        {
            var offenseIsHomeTeam = context.Environment.CurrentPlayContext!.TeamWithPossession == GameTeam.Home;
            var offenseTeam = offenseIsHomeTeam
                ? context.Environment.CurrentGameRecord!.HomeTeam
                : context.Environment.CurrentGameRecord!.AwayTeam;
            var offenseRoster = offenseIsHomeTeam
                ? context.Environment.HomeActiveRoster
                : context.Environment.AwayActiveRoster;
            var defenseTeam = offenseIsHomeTeam
                ? context.Environment.CurrentGameRecord!.AwayTeam
                : context.Environment.CurrentGameRecord!.HomeTeam;
            var defenseRoster = offenseIsHomeTeam
                ? context.Environment.AwayActiveRoster
                : context.Environment.HomeActiveRoster;
            var playInvolvement = context.Environment.CurrentPlayContext!.PlayInvolvement;
            var offensePlayersOnPlayCount = Math.Clamp(playInvolvement.OffensivePlayersInvolved, 0, 12);
            var defensePlayersOnPlayCount = Math.Clamp(playInvolvement.DefensivePlayersInvolved, 0, 11);
            var offensePlayersOnPlay = new List<PlayerRosterPosition>(offensePlayersOnPlayCount);
            var defensePlayersOnPlay = new List<PlayerRosterPosition>(defensePlayersOnPlayCount);

            if (playInvolvement.InvolvesOffensePass)
            {
                var quarterback = offenseRoster.Single(p => p.Position == BasicPlayerPosition.Quarterback);
                offensePlayersOnPlayCount -= 1;
                offensePlayersOnPlay.Add(quarterback);
            }

            if (playInvolvement.InvolvesKick)
            {
                var kicker = offenseRoster.Single(p => p.Position == BasicPlayerPosition.Kicker);
                offensePlayersOnPlayCount -= 1;
                offensePlayersOnPlay.Add(kicker);
            }

            var shuffledOffenseRoster = offenseRoster.ToList().Shuffle();
            var shuffledDefenseRoster = defenseRoster.ToList().Shuffle();
            offensePlayersOnPlay.AddRange(shuffledOffenseRoster.Take(offensePlayersOnPlayCount));
            defensePlayersOnPlay.AddRange(shuffledDefenseRoster.Take(defensePlayersOnPlayCount));

            Log.Information("DeterminePlayersOnPlayStep: Assigned {OffenseCount} offense players and {DefenseCount} defense players to the play.",
                offensePlayersOnPlay.Count, defensePlayersOnPlay.Count);
            return context.WithNextState(GameState.InjuryCheck) with
            {
                OffensePlayersOnPlay = offensePlayersOnPlay,
                DefensePlayersOnPlay = defensePlayersOnPlay
            };
        }
    }
}
