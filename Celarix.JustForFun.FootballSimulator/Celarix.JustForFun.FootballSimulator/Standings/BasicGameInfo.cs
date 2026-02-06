using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    public sealed class BasicGameInfo
    {
        public required BasicTeamInfo HomeTeam { get; init; }
        public required BasicTeamInfo AwayTeam { get; init; }
        public required int HomeScore { get; init; }
        public required int AwayScore { get; init; }
        public required int HomeTouchdowns { get; init; }
        public required int AwayTouchdowns { get; init; }

        public bool Tie => HomeScore == AwayScore;
        public BasicTeamInfo? WinningTeam =>
            Tie ? null : (HomeScore > AwayScore ? HomeTeam : AwayTeam);
        public bool IsConferenceGame =>
            HomeTeam.Conference == AwayTeam.Conference;
        public bool IsDivisionGame =>
            IsConferenceGame && HomeTeam.Division == AwayTeam.Division;

        public static BasicGameInfo FromGameRecord(GameRecord game)
        {
            var touchdownDrives = game.TeamDriveRecords.FindAll(tdr => tdr.Result.IsTouchdown());
            var homeTouchdowns = touchdownDrives.Where(tdr => tdr.TeamID == game.HomeTeamID).Count();
            var awayTouchdowns = touchdownDrives.Where(tdr => tdr.TeamID == game.AwayTeamID).Count();

            return new BasicGameInfo
            {
                HomeTeam = new BasicTeamInfo(game.HomeTeam ?? throw new InvalidOperationException("Home team is null in BasicGameInfo.FromGameRecord.")),
                AwayTeam = new BasicTeamInfo(game.AwayTeam ?? throw new InvalidOperationException("Away team is null in BasicGameInfo.FromGameRecord.")),
                HomeScore = game.HomeScore,
                AwayScore = game.AwayScore,
                HomeTouchdowns = homeTouchdowns,
                AwayTouchdowns = awayTouchdowns
            };
        }

        public int PointsFor(BasicTeamInfo team) => 
            team == HomeTeam ? HomeScore : AwayScore;

        public int PointsAgainst(BasicTeamInfo team) =>
            team == HomeTeam ? AwayScore : HomeScore;

        public BasicTeamInfo OpponentOf(BasicTeamInfo team) =>
            team == HomeTeam ? AwayTeam : HomeTeam;

        public int TouchdownsFor(BasicTeamInfo team) =>
            team == HomeTeam ? HomeTouchdowns : AwayTouchdowns;
    }
}
