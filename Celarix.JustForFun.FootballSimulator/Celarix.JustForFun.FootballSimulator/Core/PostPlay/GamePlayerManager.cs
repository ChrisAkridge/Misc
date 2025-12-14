using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.PostPlay
{
    internal sealed class GamePlayerManager
    {
        private List<Player> awayRoster;
        private List<Player> homeRoster;
        private List<Player> newPlayersForGame = new();
        private int awayTeamId;
        private int homeTeamId;
        private IRandom random;
        private PlayerManager manager;

        public GamePlayerManager(FootballContext context,
            int awayTeamId,
            int homeTeamId,
            IRandom random,
            PlayerManager manager)
        {
            this.awayTeamId = awayTeamId;
            this.homeTeamId = homeTeamId;
            this.random = random;
            this.manager = manager;

            awayRoster = LoadRosterForGame(context, awayTeamId);
            homeRoster = LoadRosterForGame(context, homeTeamId);

            FillRosterIfNeeded(awayRoster, context, awayTeamId);
            FillRosterIfNeeded(homeRoster, context, homeTeamId);
            context.SaveChanges();
        }

        private static List<Player> LoadRosterForGame(FootballContext context, int teamId)
        {
            return [.. context
                .GetActivePlayersForTeam(teamId)
                .Select(p => p.CurrentRosterPositionOnly())
                .Where(p =>
                {
                    var currentRosterPosition = p.RosterPositions.Single();
                    return !currentRosterPosition.Injured;
                })];
        }

        private void FillRosterIfNeeded(List<Player> roster, FootballContext context, int teamId)
        {
            var neededPositions = PlayerManager.GetStandardRoster().ToList();

            // Remove positions already filled
            foreach (var player in roster)
            {
                var currentRosterPosition = player.RosterPositions.Single();
                neededPositions.Remove(currentRosterPosition.Position);
            }

            // Fill needed positions
            var newPlayers = neededPositions
                .Select(p =>
                {
                    var newPlayer = manager.CreateNewPlayer(random);
                    PlayerManager.AssignPlayerToTeam(newPlayer, teamId, p, random);
                    return newPlayer;
                });
            roster.AddRange(newPlayers);
            newPlayersForGame.AddRange(newPlayers);
        }

        public IEnumerable<Player> ChoosePlayersForPlayHistory(IEnumerable<StateHistoryEntry> history)
        {
            var awayPlayerRequirements = new PlayerRequirements();
            var homePlayerRequirements = new PlayerRequirements();

            foreach (var entry in history)
            {
                var adjustments = GetPlayerRequirementsForState(entry.State);
                
                if (entry.TeamWithPossession == GameTeam.Away)
                {
                    awayPlayerRequirements.AddOffensive(adjustments.OffensivePlayers);
                    homePlayerRequirements.AddDefensive(adjustments.DefensivePlayers);
                    awayPlayerRequirements.NeedsQuarterback |= adjustments.Quarterback;
                    awayPlayerRequirements.NeedsKicker |= adjustments.Kicker;
                }
                else
                {
                    homePlayerRequirements.AddOffensive(adjustments.OffensivePlayers);
                    awayPlayerRequirements.AddDefensive(adjustments.DefensivePlayers);
                    homePlayerRequirements.NeedsQuarterback |= adjustments.Quarterback;
                    homePlayerRequirements.NeedsKicker |= adjustments.Kicker;
                }
            }

            var awaySelectedPlayers = SelectPlayers(awayRoster, awayPlayerRequirements);
            var homeSelectedPlayers = SelectPlayers(homeRoster, homePlayerRequirements);

            return awaySelectedPlayers.Concat(homeSelectedPlayers);
        }

        private static (int OffensivePlayers, int DefensivePlayers, bool Quarterback, bool Kicker) GetPlayerRequirementsForState(GameplayNextState state) => state switch
        {
            GameplayNextState.Start => (0, 0, false, false),
            GameplayNextState.KickoffDecision => (0, 0, false, true),
            GameplayNextState.FreeKickDecision => (0, 0, false, true),
            GameplayNextState.SignalFairCatchDecision => (0, 1, false, false),
            GameplayNextState.TouchdownDecision => (0, 0, false, false),
            GameplayNextState.MainGameDecision => (0, 0, false, false),
            GameplayNextState.ReturnFumbledOrInterceptedBallDecision => (0, 0, false, false),
            GameplayNextState.NormalKickoffOutcome => (0, 0, false, true),
            GameplayNextState.ReturnableKickOutcome => (1, 0, false, false),
            GameplayNextState.FumbledLiveBallOutcome => (2, 1, false, false),
            GameplayNextState.PuntOutcome => (0, 0, false, true),
            GameplayNextState.ReturnablePuntOutcome => (1, 1, false, false),
            GameplayNextState.KickOrPuntReturnOutcome => (1, 0, false, false),
            GameplayNextState.OnsideKickAttemptOutcome => (1, 1, false, true),
            GameplayNextState.FieldGoalsAndExtraPointAttemptOutcome => (1, 0, false, true),
            GameplayNextState.TwoPointConversionAttemptOutcome => (1, 1, true, false),
            GameplayNextState.FumbleOrInterceptionReturnOutcome => (1, 1, true, false),
            GameplayNextState.StandardRushingPlayOutcome => (1, 1, false, false),
            GameplayNextState.StandardShortPassingPlayOutcome => (1, 1, true, false),
            GameplayNextState.StandardMediumPassingPlayOutcome => (1, 1, false, false),
            GameplayNextState.StandardLongPassingPlayOutcome => (1, 1, false, false),
            GameplayNextState.HailMaryOutcome => (1, 1, true, false),
            GameplayNextState.QBSneakOutcome => (1, 1, true, false),
            GameplayNextState.FakePuntOutcome => (1, 1, false, true),
            GameplayNextState.FakeFieldGoalOutcome => (1, 1, false, true),
            GameplayNextState.VictoryFormationOutcome => (1, 1, true, false),
            GameplayNextState.PlayEvaluationComplete => (0, 0, false, false),
            GameplayNextState.EndOfHalf => (0, 0, false, false),
            _ => (0, 0, false, false)
        };

        private static List<Player> SelectPlayers(List<Player> roster, PlayerRequirements requirements)
        {
            var selectedPlayers = new List<Player>();
            var availablePlayers = new List<Player>(roster);

            AddPlayersByPosition(selectedPlayers, availablePlayers, BasicPlayerPosition.Offense, requirements.OffensiveCount);
            AddPlayersByPosition(selectedPlayers, availablePlayers, BasicPlayerPosition.Defense, requirements.DefensiveCount);
            
            if (requirements.NeedsQuarterback)
            {
                selectedPlayers.Add(GetPlayerByPosition(availablePlayers, BasicPlayerPosition.Quarterback));
            }
            
            if (requirements.NeedsKicker)
            {
                selectedPlayers.Add(GetPlayerByPosition(availablePlayers, BasicPlayerPosition.Kicker));
            }

            return selectedPlayers;
        }

        private static void AddPlayersByPosition(List<Player> selectedPlayers, List<Player> availablePlayers, BasicPlayerPosition position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                selectedPlayers.Add(GetPlayerByPosition(availablePlayers, position));
            }
        }

        private static Player GetPlayerByPosition(List<Player> availablePlayers, BasicPlayerPosition position)
        {
            var player = availablePlayers.Where(p => p.RosterPositions.Single().Position == position).First();
            availablePlayers.Remove(player);
            return player;
        }

        private sealed class PlayerRequirements
        {
            private const int MaxOffensivePlayers = 10;
            private const int MaxDefensivePlayers = 11;

            private int offensiveCount;
            private int defensiveCount;

            public int OffensiveCount => offensiveCount;
            public int DefensiveCount => defensiveCount;
            public bool NeedsQuarterback { get; set; }
            public bool NeedsKicker { get; set; }

            public void AddOffensive(int count)
            {
                offensiveCount = Math.Min(offensiveCount + count, MaxOffensivePlayers);
            }

            public void AddDefensive(int count)
            {
                defensiveCount = Math.Min(defensiveCount + count, MaxDefensivePlayers);
            }
        }
    }
}
