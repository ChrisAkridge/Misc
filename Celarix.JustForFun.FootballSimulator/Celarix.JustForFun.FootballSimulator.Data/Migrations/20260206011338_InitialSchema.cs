using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celarix.JustForFun.FootballSimulator.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InjuryRecoveries",
                columns: table => new
                {
                    InjuryRecoveryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    Strength = table.Column<string>(type: "TEXT", nullable: false),
                    StrengthDelta = table.Column<double>(type: "REAL", nullable: false),
                    RecoverOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Recovered = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InjuryRecoveries", x => x.InjuryRecoveryID);
                });

            migrationBuilder.CreateTable(
                name: "PhysicsParams",
                columns: table => new
                {
                    PhysicsParamID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    UnitPlural = table.Column<string>(type: "TEXT", nullable: false),
                    MinValue = table.Column<double>(type: "REAL", nullable: false),
                    MaxValue = table.Column<double>(type: "REAL", nullable: false),
                    Precision = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicsParams", x => x.PhysicsParamID);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Retired = table.Column<bool>(type: "INTEGER", nullable: false),
                    UndraftedFreeAgent = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerID);
                });

            migrationBuilder.CreateTable(
                name: "SeasonRecords",
                columns: table => new
                {
                    SeasonRecordID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonComplete = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonRecords", x => x.SeasonRecordID);
                });

            migrationBuilder.CreateTable(
                name: "SimulatorSettings",
                columns: table => new
                {
                    SimulatorSettingsID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeedDataInitialized = table.Column<bool>(type: "INTEGER", nullable: false),
                    SaveStateMachineContextsForDebugging = table.Column<bool>(type: "INTEGER", nullable: false),
                    StateMachineContextSavePath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulatorSettings", x => x.SimulatorSettingsID);
                });

            migrationBuilder.CreateTable(
                name: "Stadiums",
                columns: table => new
                {
                    StadiumID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    AverageTemperatures = table.Column<string>(type: "TEXT", nullable: false),
                    TotalPrecipitationOverSeason = table.Column<double>(type: "REAL", nullable: false),
                    AverageWindSpeed = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stadiums", x => x.StadiumID);
                });

            migrationBuilder.CreateTable(
                name: "Summaries",
                columns: table => new
                {
                    SummaryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameRecordID = table.Column<int>(type: "INTEGER", nullable: true),
                    SeasonRecordID = table.Column<int>(type: "INTEGER", nullable: true),
                    SummaryText = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Summaries", x => x.SummaryID);
                });

            migrationBuilder.CreateTable(
                name: "TeamPlayoffSeeds",
                columns: table => new
                {
                    TeamPlayoffSeedID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonRecordID = table.Column<int>(type: "INTEGER", nullable: false),
                    Seed = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamPlayoffSeeds", x => x.TeamPlayoffSeedID);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CityName = table.Column<string>(type: "TEXT", nullable: false),
                    TeamName = table.Column<string>(type: "TEXT", nullable: false),
                    Abbreviation = table.Column<string>(type: "TEXT", nullable: false),
                    Conference = table.Column<int>(type: "INTEGER", nullable: false),
                    Division = table.Column<int>(type: "INTEGER", nullable: false),
                    RunningOffenseStrength = table.Column<double>(type: "REAL", nullable: false),
                    RunningDefenseStrength = table.Column<double>(type: "REAL", nullable: false),
                    PassingOffenseStrength = table.Column<double>(type: "REAL", nullable: false),
                    PassingDefenseStrength = table.Column<double>(type: "REAL", nullable: false),
                    OffensiveLineStrength = table.Column<double>(type: "REAL", nullable: false),
                    DefensiveLineStrength = table.Column<double>(type: "REAL", nullable: false),
                    KickingStrength = table.Column<double>(type: "REAL", nullable: false),
                    FieldGoalStrength = table.Column<double>(type: "REAL", nullable: false),
                    KickReturnStrength = table.Column<double>(type: "REAL", nullable: false),
                    KickDefenseStrength = table.Column<double>(type: "REAL", nullable: false),
                    ClockManagementStrength = table.Column<double>(type: "REAL", nullable: false),
                    Disposition = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeStadiumID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamID);
                    table.ForeignKey(
                        name: "FK_Teams_Stadiums_HomeStadiumID",
                        column: x => x.HomeStadiumID,
                        principalTable: "Stadiums",
                        principalColumn: "StadiumID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameRecords",
                columns: table => new
                {
                    GameID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonRecordID = table.Column<int>(type: "INTEGER", nullable: false),
                    GameType = table.Column<int>(type: "INTEGER", nullable: false),
                    WeekNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    GameComplete = table.Column<bool>(type: "INTEGER", nullable: false),
                    HomeTeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    AwayTeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    StadiumID = table.Column<int>(type: "INTEGER", nullable: false),
                    KickoffTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    TemperatureAtKickoff = table.Column<double>(type: "REAL", nullable: false),
                    WeatherAtKickoff = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeTeamStrengthsAtKickoffJSON = table.Column<string>(type: "TEXT", nullable: true),
                    AwayTeamStrengthsAtKickoffJSON = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRecords", x => x.GameID);
                    table.ForeignKey(
                        name: "FK_GameRecords_SeasonRecords_SeasonRecordID",
                        column: x => x.SeasonRecordID,
                        principalTable: "SeasonRecords",
                        principalColumn: "SeasonRecordID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRecords_Stadiums_StadiumID",
                        column: x => x.StadiumID,
                        principalTable: "Stadiums",
                        principalColumn: "StadiumID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRecords_Teams_AwayTeamID",
                        column: x => x.AwayTeamID,
                        principalTable: "Teams",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRecords_Teams_HomeTeamID",
                        column: x => x.HomeTeamID,
                        principalTable: "Teams",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerRosterPositions",
                columns: table => new
                {
                    PlayerRosterPositionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentPlayer = table.Column<bool>(type: "INTEGER", nullable: false),
                    JerseyNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesUntilReturnFromInjury = table.Column<int>(type: "INTEGER", nullable: true),
                    TeamWins = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamTies = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamPassingYards = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamInterceptions = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamPassAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamPassCompletions = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamRushingAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamRushingYards = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamReceivingYards = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamTackles = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamSacks = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamKickReturns = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamReturnYards = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamKickoffs = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamTouchbacks = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamTouchdownsScored = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamFieldGoalAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamFieldGoalsMade = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamExtraPointAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamExtraPointsMade = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamLongFieldGoal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRosterPositions", x => x.PlayerRosterPositionID);
                    table.ForeignKey(
                        name: "FK_PlayerRosterPositions_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerRosterPositions_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuarterBoxScores",
                columns: table => new
                {
                    QuarterBoxScoreID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameRecordID = table.Column<int>(type: "INTEGER", nullable: false),
                    QuarterNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Team = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuarterBoxScores", x => x.QuarterBoxScoreID);
                    table.ForeignKey(
                        name: "FK_QuarterBoxScores_GameRecords_GameRecordID",
                        column: x => x.GameRecordID,
                        principalTable: "GameRecords",
                        principalColumn: "GameID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamDriveRecords",
                columns: table => new
                {
                    TeamDriveRecordID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameRecordID = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    Team = table.Column<int>(type: "INTEGER", nullable: false),
                    QuarterNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DriveStartTimeSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    StartingFieldPosition = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DriveDurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    NetYards = table.Column<int>(type: "INTEGER", nullable: false),
                    Result = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamDriveRecords", x => x.TeamDriveRecordID);
                    table.ForeignKey(
                        name: "FK_TeamDriveRecords_GameRecords_GameRecordID",
                        column: x => x.GameRecordID,
                        principalTable: "GameRecords",
                        principalColumn: "GameID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamGameRecords",
                columns: table => new
                {
                    TeamGameRecordID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameRecordID = table.Column<int>(type: "INTEGER", nullable: false),
                    Team = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstDowns = table.Column<int>(type: "INTEGER", nullable: false),
                    RushAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    RushYards = table.Column<int>(type: "INTEGER", nullable: false),
                    RushTouchdowns = table.Column<int>(type: "INTEGER", nullable: false),
                    PassAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    PassCompletions = table.Column<int>(type: "INTEGER", nullable: false),
                    PassYards = table.Column<int>(type: "INTEGER", nullable: false),
                    PassTouchdowns = table.Column<int>(type: "INTEGER", nullable: false),
                    PassInterceptions = table.Column<int>(type: "INTEGER", nullable: false),
                    Sacks = table.Column<int>(type: "INTEGER", nullable: false),
                    SackYards = table.Column<int>(type: "INTEGER", nullable: false),
                    Fumbles = table.Column<int>(type: "INTEGER", nullable: false),
                    FumblesLost = table.Column<int>(type: "INTEGER", nullable: false),
                    Penalties = table.Column<int>(type: "INTEGER", nullable: false),
                    PenaltyYards = table.Column<int>(type: "INTEGER", nullable: false),
                    ThirdDownConversionAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageThirdDownDistance = table.Column<double>(type: "REAL", nullable: false),
                    ThirdDownConversions = table.Column<int>(type: "INTEGER", nullable: false),
                    FourthDownConversionAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageFourthDownDistance = table.Column<double>(type: "REAL", nullable: false),
                    FourthDownConversions = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldGoalAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldGoalsMade = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraPointAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraPointAttemptsMade = table.Column<int>(type: "INTEGER", nullable: false),
                    TwoPointConversionAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    TwoPointConversionAttemptsMade = table.Column<int>(type: "INTEGER", nullable: false),
                    Punts = table.Column<int>(type: "INTEGER", nullable: false),
                    PuntYards = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeOfPossessionSeconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamGameRecords", x => x.TeamGameRecordID);
                    table.ForeignKey(
                        name: "FK_TeamGameRecords_GameRecords_GameRecordID",
                        column: x => x.GameRecordID,
                        principalTable: "GameRecords",
                        principalColumn: "GameID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameRecords_AwayTeamID",
                table: "GameRecords",
                column: "AwayTeamID");

            migrationBuilder.CreateIndex(
                name: "IX_GameRecords_HomeTeamID",
                table: "GameRecords",
                column: "HomeTeamID");

            migrationBuilder.CreateIndex(
                name: "IX_GameRecords_SeasonRecordID",
                table: "GameRecords",
                column: "SeasonRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_GameRecords_StadiumID",
                table: "GameRecords",
                column: "StadiumID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRosterPositions_PlayerID",
                table: "PlayerRosterPositions",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRosterPositions_TeamID",
                table: "PlayerRosterPositions",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_QuarterBoxScores_GameRecordID",
                table: "QuarterBoxScores",
                column: "GameRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamDriveRecords_GameRecordID",
                table: "TeamDriveRecords",
                column: "GameRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamGameRecords_GameRecordID",
                table: "TeamGameRecords",
                column: "GameRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_HomeStadiumID",
                table: "Teams",
                column: "HomeStadiumID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InjuryRecoveries");

            migrationBuilder.DropTable(
                name: "PhysicsParams");

            migrationBuilder.DropTable(
                name: "PlayerRosterPositions");

            migrationBuilder.DropTable(
                name: "QuarterBoxScores");

            migrationBuilder.DropTable(
                name: "SimulatorSettings");

            migrationBuilder.DropTable(
                name: "Summaries");

            migrationBuilder.DropTable(
                name: "TeamDriveRecords");

            migrationBuilder.DropTable(
                name: "TeamGameRecords");

            migrationBuilder.DropTable(
                name: "TeamPlayoffSeeds");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "GameRecords");

            migrationBuilder.DropTable(
                name: "SeasonRecords");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Stadiums");
        }
    }
}
