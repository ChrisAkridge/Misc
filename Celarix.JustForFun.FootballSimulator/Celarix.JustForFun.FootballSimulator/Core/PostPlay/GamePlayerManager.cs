using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.PostPlay
{
    internal sealed class GamePlayerManager
    {
        public sealed class PlayerCounts
        {
            // 10 + quarterback, who is stored separately
            public const int MaxOffensivePlayers = 10;
            public const int MaxDefensivePlayers = 11;

            private int offensiveCount;
            private int defensiveCount;

            public int OffensiveCount => offensiveCount;
            public int DefensiveCount => defensiveCount;
            public bool Quarterback { get; set; }
            public bool Kicker { get; set; }

            public bool Empty => offensiveCount == 0 && defensiveCount == 0 && !Quarterback && !Kicker;

            public void AddOffensive(int count)
            {
                offensiveCount = Math.Min(offensiveCount + count, MaxOffensivePlayers);
            }

            public void AddDefensive(int count)
            {
                defensiveCount = Math.Min(defensiveCount + count, MaxDefensivePlayers);
            }

            public override string ToString()
            {
                return $"{OffensiveCount} offense, {DefensiveCount} defense, QB: {Quarterback}, K: {Kicker}";
            }
        }

        private readonly FootballContext context;
        private List<Player> awayRoster;
        private List<Player> homeRoster;
        private double awayAverageAcclimatedTemperature;
        private double homeAverageAcclimatedTemperature;
        private List<Player> newPlayersForGame = new();
        private int awayTeamId;
        private int homeTeamId;
        private IRandom random;
        private PlayerManager manager;

        public double StadiumCurrentTemperature { get; set; }

        public GamePlayerManager(FootballContext context,
            int awayTeamId,
            int homeTeamId,
            IRandom random,
            PlayerManager manager,
            int gameStadiumID)
        {
            this.context = context;
            this.awayTeamId = awayTeamId;
            this.homeTeamId = homeTeamId;
            this.random = random;
            this.manager = manager;

            awayRoster = LoadRosterForGame(context, awayTeamId);
            homeRoster = LoadRosterForGame(context, homeTeamId);

            FillRosterIfNeeded(awayRoster, context, awayTeamId);
            FillRosterIfNeeded(homeRoster, context, homeTeamId);
            context.SaveChanges();

            var gameStadium = context.Stadiums.Single(s => s.StadiumID == gameStadiumID);
            var awayTeamHomeStadium = context.GetHomeStadiumForTeam(awayTeamId);
            var homeTeamHomeStadium = context.GetHomeStadiumForTeam(homeTeamId);
            awayAverageAcclimatedTemperature = GetAverageAcclimatedTemperatureForStadium(awayTeamHomeStadium);
            homeAverageAcclimatedTemperature = GetAverageAcclimatedTemperatureForStadium(homeTeamHomeStadium);
        }

        public IEnumerable<Player> ChoosePlayersForPlayHistory(IEnumerable<StateHistoryEntry> history)
        {
            var awayPlayerRequirements = new PlayerCounts();
            var homePlayerRequirements = new PlayerCounts();

            foreach (var entry in history)
            {
                var adjustments = GetPlayerRequirementsForState(entry.State);

                if (entry.TeamWithPossession == GameTeam.Away)
                {
                    awayPlayerRequirements.AddOffensive(adjustments.OffensivePlayers);
                    homePlayerRequirements.AddDefensive(adjustments.DefensivePlayers);
                    awayPlayerRequirements.Quarterback |= adjustments.Quarterback;
                    awayPlayerRequirements.Kicker |= adjustments.Kicker;
                }
                else
                {
                    homePlayerRequirements.AddOffensive(adjustments.OffensivePlayers);
                    awayPlayerRequirements.AddDefensive(adjustments.DefensivePlayers);
                    homePlayerRequirements.Quarterback |= adjustments.Quarterback;
                    homePlayerRequirements.Kicker |= adjustments.Kicker;
                }
            }

            var awaySelectedPlayers = SelectPlayers(awayRoster, awayPlayerRequirements);
            var homeSelectedPlayers = SelectPlayers(homeRoster, homePlayerRequirements);

            return awaySelectedPlayers.Concat(homeSelectedPlayers);
        }

        public (PlayerCounts AwayTeamInjuredCount, PlayerCounts HomeTeamInjuredCount) DoInjuryCheck(IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            // 0. Start a PlayerCounts object to return
            var awayInjuredCount = new PlayerCounts();
            var homeInjuredCount = new PlayerCounts();

            // 1. Get BaseInjuryChancePerPlay physics param
            var baseInjuryChanceParam = physicsParams["BaseInjuryChancePerPlay"].Value;

            var awayTemperatureDifference = Math.Abs(StadiumCurrentTemperature - awayAverageAcclimatedTemperature);
            var awayInjuryChance = baseInjuryChanceParam * Math.Pow(1.1, awayTemperatureDifference / 5.0);
            var injuredAwayPlayers = new List<Player>();

            foreach (var player in awayRoster)
            {
                if (random.NextDouble() < awayInjuryChance)
                {
                    injuredAwayPlayers.Add(player);
                }
            }

            foreach (var player in injuredAwayPlayers)
            {
                var position = player.RosterPositions.Single().Position;
                if (position == BasicPlayerPosition.Offense)
                {
                    awayInjuredCount.AddOffensive(1);
                }
                else if (position == BasicPlayerPosition.Defense)
                {
                    awayInjuredCount.AddDefensive(1);
                }
                else if (position == BasicPlayerPosition.Quarterback)
                {
                    awayInjuredCount.Quarterback = true;
                }
                else if (position == BasicPlayerPosition.Kicker)
                {
                    awayInjuredCount.Kicker = true;
                }
            }
            Log.Verbose("Injury: {InjuredCount} for away", awayInjuredCount);

            var homeTemperatureDifference = Math.Abs(StadiumCurrentTemperature - homeAverageAcclimatedTemperature);
            var homeInjuryChance = baseInjuryChanceParam * Math.Pow(1.1, homeTemperatureDifference / 5.0);
            var injuredHomePlayers = new List<Player>();

            foreach (var player in homeRoster)
            {
                if (random.NextDouble() < homeInjuryChance)
                {
                    injuredHomePlayers.Add(player);
                }
            }

            foreach (var player in injuredHomePlayers)
            {
                var position = player.RosterPositions.Single().Position;
                if (position == BasicPlayerPosition.Offense)
                {
                    homeInjuredCount.AddOffensive(1);
                }
                else if (position == BasicPlayerPosition.Defense)
                {
                    homeInjuredCount.AddDefensive(1);
                }
                else if (position == BasicPlayerPosition.Quarterback)
                {
                    homeInjuredCount.Quarterback = true;
                }
                else if (position == BasicPlayerPosition.Kicker)
                {
                    homeInjuredCount.Kicker = true;
                }
            }
            Log.Verbose("Injury: {InjuredCount} for home", homeInjuredCount);

            foreach (var player in injuredAwayPlayers)
            {
                InjurePlayer(player, physicsParams);
                awayRoster.Remove(player);
            }

            foreach (var player in injuredHomePlayers)
            {
                InjurePlayer(player, physicsParams);
                homeRoster.Remove(player);
            }

            FillRosterIfNeeded(awayRoster, context, awayTeamId);
            FillRosterIfNeeded(homeRoster, context, homeTeamId);
            context.SaveChanges();
            return (awayInjuredCount, homeInjuredCount);
        }

        public void CompleteGame()
        {
            foreach (var player in newPlayersForGame)
            {
                var rosterPosition = player.RosterPositions.Single();
                rosterPosition.CurrentPlayer = false;
            }

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
                    var newPlayer = manager.CreateNewPlayer(random, undraftedFreeAgent: true);
                    PlayerManager.AssignPlayerToTeam(newPlayer, teamId, p, random);
                    return newPlayer;
                });
            roster.AddRange(newPlayers);
            newPlayersForGame.AddRange(newPlayers);
        }

        private static double GetAverageAcclimatedTemperatureForStadium(Stadium stadium)
        {
            var monthlyTemperatures = stadium.AverageTemperatures
                .Split(',')
                .Select(double.Parse)
                .ToList();
            return monthlyTemperatures.Average();
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

        private static List<Player> SelectPlayers(List<Player> roster, PlayerCounts requirements)
        {
            var selectedPlayers = new List<Player>();
            var availablePlayers = new List<Player>(roster);

            AddPlayersByPosition(selectedPlayers, availablePlayers, BasicPlayerPosition.Offense, requirements.OffensiveCount);
            AddPlayersByPosition(selectedPlayers, availablePlayers, BasicPlayerPosition.Defense, requirements.DefensiveCount);
            
            if (requirements.Quarterback)
            {
                selectedPlayers.Add(GetPlayerByPosition(availablePlayers, BasicPlayerPosition.Quarterback));
            }
            
            if (requirements.Kicker)
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
            Log.Verbose("Selected player {FirstName} {LastName} ({Position})", player.FirstName, player.LastName, position);
            return player;
        }

        private void InjurePlayer(Player player, IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            // Sorry!

            var falloff = physicsParams["InjuryDurationAdditionalWeekFalloff"].Value;
            var injuryDurationWeeks = 1;
            var chance = falloff;

            while (random.Chance(chance))
            {
                injuryDurationWeeks++;
                chance *= falloff;
            }

            var currentRosterPosition = player.RosterPositions.Single();
            currentRosterPosition.GamesUntilReturnFromInjury = injuryDurationWeeks;
            Log.Verbose("Injury: Player {FirstName} {LastName} (team #{TeamName}) injured for {Duration} weeks",
                player.FirstName, player.LastName, currentRosterPosition.TeamID, injuryDurationWeeks);
        }
    }
}
