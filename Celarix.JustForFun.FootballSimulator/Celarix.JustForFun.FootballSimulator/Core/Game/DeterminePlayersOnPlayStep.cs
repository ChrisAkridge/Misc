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
            var random = context.Environment.RandomFactory.Create();

            var offenseIsHomeTeam = context.Environment.CurrentPlayContext!.TeamWithPossession == GameTeam.Home;
            var offenseTeam = offenseIsHomeTeam
                ? context.Environment.CurrentGameRecord!.HomeTeam ?? throw new InvalidOperationException("Home team not loaded from database.")
                : context.Environment.CurrentGameRecord!.AwayTeam ?? throw new InvalidOperationException("Away team not loaded from database.");
            var offenseRoster = offenseIsHomeTeam
                ? context.Environment.HomeActiveRoster
                : context.Environment.AwayActiveRoster;
            var defenseTeam = offenseIsHomeTeam
                ? context.Environment.CurrentGameRecord!.AwayTeam ?? throw new InvalidOperationException("Away team not loaded from database.")
                : context.Environment.CurrentGameRecord!.HomeTeam ?? throw new InvalidOperationException("Home team not loaded from database.");
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

            var shuffledOffenseRoster = offenseRoster.ToList().Shuffle(random);
            var shuffledDefenseRoster = defenseRoster.ToList().Shuffle(random);
            offensePlayersOnPlay.AddRange(shuffledOffenseRoster.Take(offensePlayersOnPlayCount));
            defensePlayersOnPlay.AddRange(shuffledDefenseRoster.Take(defensePlayersOnPlayCount));

            var lineOfScrimmageTeamYard = context.Environment.CurrentPlayContext.InternalYardToTeamYard(context.Environment.CurrentPlayContext.LineOfScrimmage);
            var teamYardTeamAbbreviation = lineOfScrimmageTeamYard.Team == GameTeam.Home
                ? context.Environment.CurrentGameRecord!.HomeTeam?.Abbreviation ?? throw new InvalidOperationException("Home team not loaded from database.")
                : context.Environment.CurrentGameRecord!.AwayTeam?.Abbreviation ?? throw new InvalidOperationException("Away team not loaded from database.");
            var lineOfScrimmageDisplay = $"{teamYardTeamAbbreviation} {lineOfScrimmageTeamYard.TeamYard}";
            context.Environment.CurrentPlayContext = context.Environment.CurrentPlayContext with
            {
                LastPlayDescriptionTemplate = ResolveLastPlayDescriptionTemplate(
                    context.Environment.CurrentPlayContext.LastPlayDescriptionTemplate,
                    offensePlayersOnPlay,
                    defensePlayersOnPlay,
                    offenseTeam.Abbreviation,
                    defenseTeam.Abbreviation,
                    lineOfScrimmageDisplay)
            };

            Log.Information("DeterminePlayersOnPlayStep: Assigned {OffenseCount} offense players and {DefenseCount} defense players to the play.",
                offensePlayersOnPlay.Count, defensePlayersOnPlay.Count);
            return context.WithNextState(GameState.InjuryCheck) with
            {
                OffensePlayersOnPlay = offensePlayersOnPlay,
                DefensePlayersOnPlay = defensePlayersOnPlay
            };
        }

        private static string ResolveLastPlayDescriptionTemplate(string template,
            IReadOnlyList<PlayerRosterPosition> offensePlayersOnPlay,
            IReadOnlyList<PlayerRosterPosition> defensePlayersOnPlay,
            string offenseAbbreviation,
            string defenseAbbreviation,
            string lineOfScrimmageTeamYard)
        {
            if (offensePlayersOnPlay.Concat(defensePlayersOnPlay)
                    .Any(p => p.Player == null))
            {
                throw new InvalidOperationException("One or more PlayerRosterPosition instances are missing their Player reference.");
            }

            var description = template;
            for (int i = 0; i < offensePlayersOnPlay.Count; i++)
            {
                description = description.Replace($"{{OffPlayer{i}}}", offensePlayersOnPlay[i].Player!.FullName);
            }
            for (int i = 0; i < defensePlayersOnPlay.Count; i++)
            {
                description = description.Replace($"{{DefPlayer{i}}}", defensePlayersOnPlay[i].Player!.FullName);
            }

            description = description.Replace("{OffAbbr}", offenseAbbreviation);
            description = description.Replace("{DefAbbr}", defenseAbbreviation);
            description = description.Replace("{LoS}", lineOfScrimmageTeamYard);
            return description;
        }
    }
}
