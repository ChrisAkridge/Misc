using Celarix.JustForFun.FootballSimulator.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data
{
    public sealed class FootballRepository(FootballContext context) : IFootballRepository
    {
        private readonly FootballContext context = context;

        public void EnsureCreated()
        {
            context.Database.EnsureCreated();
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public SeasonRecord? GetMostRecentSeason()
        {
            return context.SeasonRecords
                .OrderByDescending(sr => sr.Year)
                .FirstOrDefault();
        }

        public IReadOnlyList<GameRecord> GetPlayoffGamesForSeason(int seasonID, PlayoffRound playoffRound)
        {
            var week = playoffRound switch
            {
                PlayoffRound.WildCard => 18,
                PlayoffRound.Divisional => 19,
                PlayoffRound.ConferenceChampionship => 20,
                PlayoffRound.SuperBowl => 22,
                _ => throw new ArgumentOutOfRangeException(nameof(playoffRound), "Invalid playoff round specified.")
            };

            return [.. context.GameRecords
                .Include(g => g.AwayTeam)
                .Include(g => g.HomeTeam)
                .Where(gr => gr.SeasonRecordID == seasonID
                    && gr.GameType == GameType.Postseason
                    && gr.WeekNumber == week)];
        }

        public IReadOnlyList<TeamPlayoffSeed> GetPlayoffSeedsForSeason(int seasonID)
        {
            return [.. context.TeamPlayoffSeeds
                .Where(tps => tps.SeasonRecordID == seasonID)];
        }

        public void AddGameRecord(GameRecord gameRecord)
        {
            context.GameRecords.Add(gameRecord);
        }

        public void AddGameRecords(IEnumerable<GameRecord> gameRecords)
        {
            context.GameRecords.AddRange(gameRecords);
        }

        public SimulatorSettings? GetSimulatorSettings()
        {
            return context.SimulatorSettings.SingleOrDefault();
        }

        public void AddSimulatorSettings(SimulatorSettings settings)
        {
            context.SimulatorSettings.Add(settings);
        }

        public void AddTeam(Team team)
        {
            context.Teams.Add(team);
        }

        public void AddPlayer(Player player)
        {
            context.Players.Add(player);
        }

        public void AddPhysicsParam(PhysicsParam param)
        {
            context.PhysicsParams.Add(param);
        }

        public IReadOnlyList<Team> GetTeams()
        {
            return [.. context.Teams];
        }

        public IReadOnlyList<GameRecord> GetGameRecordsForSeasonByGameType(int seasonID, GameType gameType)
        {
            return [.. context.GameRecords
                .Where(gr => gr.SeasonRecordID == seasonID
                    && gr.GameType == gameType)];
        }

        public void AddSeasonRecord(SeasonRecord seasonRecord)
        {
            context.SeasonRecords.Add(seasonRecord);
        }

        public void AddTeamPlayoffSeeds(IEnumerable<TeamPlayoffSeed> seeds)
        {
            context.TeamPlayoffSeeds.AddRange(seeds);
        }

        public IReadOnlyList<PhysicsParam> GetPhysicsParams()
        {
            return [.. context.PhysicsParams];
        }

        public GameRecord? GetNextUnplayedGame()
        {
            return context.GameRecords
                .Include(g => g.Stadium)
                .Include(g => g.TeamDriveRecords)
                .OrderBy(g => g.KickoffTime)
                .FirstOrDefault(g => !g.GameComplete);
        }

        public IReadOnlyList<InjuryRecovery> GetInjuryRecoveriesForGame(int awayTeamID, int homeTeamID, DateTimeOffset kickoffTime)
        {
            return [.. context.InjuryRecoveries
                .Where(ir => (ir.TeamID == awayTeamID || ir.TeamID == homeTeamID)
                    && ir.RecoverOn >= kickoffTime)];
        }

        public int CountIncompleteSeasons()
        {
            return context.SeasonRecords.Count(sr => !sr.SeasonComplete);
        }

        public Summary? GetSummaryForSeason(int seasonRecordID)
        {
            return context.Summaries
                .SingleOrDefault(s => s.SeasonRecordID == seasonRecordID
                    && s.GameRecordID == null);
        }

        public IReadOnlyList<GameRecord> GetGameRecordsForSeason(int seasonID)
        {
            return [.. context.GameRecords
                .Include(g => g.TeamDriveRecords)
                .Where(gr => gr.SeasonRecordID == seasonID)];
        }

        public GameRecord GetPartialGameRecord()
        {
            return context.GameRecords
                .Include(g => g.Stadium)
                .Include(g => g.TeamDriveRecords)
                .Single(g => !g.GameComplete && g.TeamDriveRecords.Count > 0);
        }

        public void AddSummary(Summary summary)
        {
            context.Summaries.Add(summary);
        }

        public SeasonRecord GetSeasonWithGames(int seasonRecordID)
        {
            return context.SeasonRecords
                .Include(sr => sr.GameRecords!)
                    .ThenInclude(gr => gr.HomeTeam)
                .Include(sr => sr.GameRecords!)
                    .ThenInclude(gr => gr.AwayTeam)
                .Include(sr => sr.GameRecords!)
                    .ThenInclude(gr => gr.QuarterBoxScores)
                .Single(sr => sr.SeasonRecordID == seasonRecordID);
        }

        public IReadOnlyList<PlayerRosterPosition> GetActiveRosterForTeam(int teamID)
        {
            return [.. context.PlayerRosterPositions
                .Include(prp => prp.Player)
                .Where(prp => prp.TeamID == teamID && prp.CurrentPlayer)];
        }

        public void AddPlayerRosterPosition(PlayerRosterPosition position)
        {
            context.PlayerRosterPositions.Add(position);
        }

        public void AddInjuryRecoveries(IEnumerable<InjuryRecovery> recoveries)
        {
            context.InjuryRecoveries.AddRange(recoveries);
        }

        public void AddTeamDriveRecord(TeamDriveRecord driveRecord)
        {
            context.TeamDriveRecords.Add(driveRecord);
        }

        public int GetScoreForTeamInGame(int gameRecordID, GameTeam gameTeam)
        {
            return context.QuarterBoxScores
                .Where(qbs => qbs.GameRecordID == gameRecordID && qbs.Team == gameTeam)
                .Sum(qbs => qbs.Score);
        }

        public void AddQuarterBoxScore(QuarterBoxScore quarterBoxScore)
        {
            context.QuarterBoxScores.Add(quarterBoxScore);
        }

        public void CompleteGame(int gameRecordID)
        {
            var gameRecord = context.GameRecords.Single(gr => gr.GameID == gameRecordID);
            gameRecord.GameComplete = true;
            context.GameRecords.Update(gameRecord);
        }

        public void SetTeamStrengths(IStrengths newStrengths, int teamID)
        {
            var team = context.Teams.Single(t => t.TeamID == teamID);
            team.OffensiveLineStrength = newStrengths.OffensiveLineStrength;
            team.DefensiveLineStrength = newStrengths.DefensiveLineStrength;
            team.RunningOffenseStrength = newStrengths.RunningOffenseStrength;
            team.RunningDefenseStrength = newStrengths.RunningDefenseStrength;
            team.PassingOffenseStrength = newStrengths.PassingOffenseStrength;
            team.PassingDefenseStrength = newStrengths.PassingDefenseStrength;
            team.KickingStrength = newStrengths.KickingStrength;
            team.FieldGoalStrength = newStrengths.FieldGoalStrength;
            team.KickReturnStrength = newStrengths.KickReturnStrength;
            team.KickDefenseStrength = newStrengths.KickDefenseStrength;
            context.Teams.Update(team);
        }

        public Stadium GetStadium(int stadiumID)
        {
            return context.Stadiums.Single(s => s.StadiumID == stadiumID);
        }
    }
}
