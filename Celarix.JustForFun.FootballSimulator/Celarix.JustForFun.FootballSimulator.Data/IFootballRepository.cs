using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data
{
    public interface IFootballRepository
    {
        void EnsureCreated();
        void SaveChanges();
        SeasonRecord? GetMostRecentSeason();
        IReadOnlyList<GameRecord> GetPlayoffGamesForSeason(int seasonID, PlayoffRound playoffRound);
        IReadOnlyList<TeamPlayoffSeed> GetPlayoffSeedsForSeason(int seasonID);
        void AddGameRecord(GameRecord gameRecord);
        void AddGameRecords(IEnumerable<GameRecord> gameRecords);
        SimulatorSettings? GetSimulatorSettings();
        void AddSimulatorSettings(SimulatorSettings settings);
        void AddTeam(Team team);
        void AddPlayer(Player player);
        void AddPhysicsParam(PhysicsParam param);
        IReadOnlyList<Team> GetTeams();
        IReadOnlyList<GameRecord> GetGameRecordsForSeasonByGameType(int seasonID, GameType gameType);
        void AddSeasonRecord(SeasonRecord seasonRecord);
        void AddTeamPlayoffSeeds(IEnumerable<TeamPlayoffSeed> seeds);
        IReadOnlyList<PhysicsParam> GetPhysicsParams();
        GameRecord? GetNextUnplayedGame();
        IReadOnlyList<InjuryRecovery> GetInjuryRecoveriesForGame(int awayTeamID, int homeTeamID, DateTimeOffset kickoffTime);
        int CountIncompleteSeasons();
        Summary? GetSummaryForSeason(int seasonRecordID);
        IReadOnlyList<GameRecord> GetGameRecordsForSeason(int seasonID);
        GameRecord GetPartialGameRecord();
        void AddSummary(Summary summary);
        SeasonRecord GetSeasonWithGames(int seasonRecordID);
        IReadOnlyList<PlayerRosterPosition> GetActiveRosterForTeam(int teamID);
        void AddPlayerRosterPosition(PlayerRosterPosition position);
        void AddInjuryRecoveries(IEnumerable<InjuryRecovery> recoveries);
        void AddTeamDriveRecord(TeamDriveRecord driveRecord);
        int GetScoreForTeamInGame(int gameRecordID, GameTeam gameTeam);
        void AddQuarterBoxScore(QuarterBoxScore quarterBoxScore);
        void CompleteGame(int gameRecordID);
        void SetTeamStrengths(IStrengths newStrengths, int teamID);
        Stadium GetStadium(int stadiumID);
    }
}
