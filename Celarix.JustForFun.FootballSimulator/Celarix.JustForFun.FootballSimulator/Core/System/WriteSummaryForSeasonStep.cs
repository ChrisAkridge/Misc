using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.SummaryWriting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class WriteSummaryForSeasonStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var summaryWriter = context.Environment.SummaryWriter;
            var currentSeasonID = context.Environment.CurrentGameRecord!.SeasonRecordID;
            var currentSeason = repository.GetSeasonWithGames(currentSeasonID);
            var allTeams = repository.GetTeams();

            // Write season summary
            var seasonSummaryText = summaryWriter.WriteSeasonSummary(currentSeason, allTeams);
            var summary = new Summary
            {
                SeasonRecordID = currentSeasonID,
                SummaryText = seasonSummaryText
            };
            repository.AddSummary(summary);

            // Perform offseason trades
            PerformTrades(context);

            // Mark season as complete
            currentSeason.SeasonComplete = true;
            repository.SaveChanges();
            Log.Information("WriteSummaryForSeasonStep: Completed the {SeasonYear} season.", currentSeason.Year);
            return context.WithNextState(SystemState.PrepareForGame);
        }

        internal static void PerformTrades(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var physicsParams = repository
                .GetPhysicsParams()
                .ToDictionary(pp => pp.Name, pp => pp);
            var allTeams = repository.GetTeams();
            var allTeamRosters = allTeams.ToDictionary(
                team => team.TeamID,
                team => repository.GetActiveRosterForTeam(team.TeamID).ToList());
            var tradePool = new List<PlayerRosterPosition>();
            var random = context.Environment.RandomFactory.Create();

            foreach (var team in allTeams)
            {
                var teamRoster = allTeamRosters[team.TeamID];
                var numTrades = (int)random.SampleNormalDistribution(physicsParams["TeamPlayersTradedPerOffseasonMean"].Value,
                    physicsParams["TeamPlayersTradedPerOffseasonStdDev"].Value);
                for (int i = 0; i < numTrades && teamRoster.Count > 0; i++)
                {
                    var playerIndex = random.Next(teamRoster.Count);
                    var playerToTrade = teamRoster[playerIndex];
                    tradePool.Add(playerToTrade);
                    playerToTrade.CurrentPlayer = false;
                    teamRoster.RemoveAt(playerIndex);
                }
            }
            
            tradePool.Shuffle(random);
            foreach (var team in allTeams)
            {
                var teamRoster = allTeamRosters[team.TeamID];
                if (teamRoster.Count >= 23)
                {
                    continue;
                }

                var neededPlayers = 23 - teamRoster.Count;
                for (int i = 0; i < neededPlayers; i++)
                {
                    PlayerRosterPosition newRosterPosition;
                    if (tradePool.Count > 0)
                    {
                        var playerToAcquire = tradePool[0];
                        tradePool.RemoveAt(0);
                        newRosterPosition = new PlayerRosterPosition
                        {
                            PlayerID = playerToAcquire.PlayerID,
                            TeamID = team.TeamID,
                            CurrentPlayer = true,
                            JerseyNumber = random.Next(0, 100),
                            Position = playerToAcquire.Position
                        };
                        repository.AddPlayerRosterPosition(newRosterPosition);
                    }
                    else
                    {
                        // Draft new player
                        var newPlayer = context.Environment.PlayerFactory.CreateNewPlayer(random, undraftedFreeAgent: false);
                        repository.AddPlayer(newPlayer);
                        newRosterPosition = new PlayerRosterPosition
                        {
                            PlayerID = newPlayer.PlayerID,
                            TeamID = team.TeamID,
                            CurrentPlayer = true,
                            JerseyNumber = random.Next(0, 100),
                            Position = FirstMissingPosition(teamRoster)
                        };
                        repository.AddPlayerRosterPosition(newRosterPosition);
                    }
                    teamRoster.Add(newRosterPosition);
                }
            }
        }

        internal static BasicPlayerPosition FirstMissingPosition(IReadOnlyList<PlayerRosterPosition> roster)
        {
            var playerCounts = new Dictionary<BasicPlayerPosition, int>
            {
                { BasicPlayerPosition.Quarterback, 0 },
                { BasicPlayerPosition.Offense, 0 },
                { BasicPlayerPosition.Defense, 0 },
                { BasicPlayerPosition.Kicker, 0 }
            };

            foreach (var rosterPosition in roster)
            {
                playerCounts[rosterPosition.Position]++;
            }

            if (playerCounts[BasicPlayerPosition.Quarterback] < 1)
            {
                return BasicPlayerPosition.Quarterback;
            }
            else if (playerCounts[BasicPlayerPosition.Offense] < 10)
            {
                return BasicPlayerPosition.Offense;
            }
            else if (playerCounts[BasicPlayerPosition.Defense] < 11)
            {
                return BasicPlayerPosition.Defense;
            }
            else if (playerCounts[BasicPlayerPosition.Kicker] < 1)
            {
                return BasicPlayerPosition.Kicker;
            }
            else
            {
                throw new InvalidOperationException("Roster is complete; no missing positions.");
            }
        }
    }
}
