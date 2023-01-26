using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celarix.JustForFun.FootballSimulator.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonRecords",
                columns: table => new
                {
                    SeasonRecordID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonRecords", x => x.SeasonRecordID);
                });

            migrationBuilder.CreateTable(
                name: "Stadium",
                columns: table => new
                {
                    StadiumID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    AverageTemperature = table.Column<double>(type: "REAL", nullable: false),
                    OddsOfPrecipitation = table.Column<double>(type: "REAL", nullable: false),
                    AverageWindSpeed = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stadium", x => x.StadiumID);
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
                        name: "FK_Teams_Stadium_HomeStadiumID",
                        column: x => x.HomeStadiumID,
                        principalTable: "Stadium",
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
                    HomeTeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    AwayTeamID = table.Column<int>(type: "INTEGER", nullable: false),
                    StadiumID = table.Column<int>(type: "INTEGER", nullable: false),
                    KickoffTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    TemperatureAtKickoff = table.Column<double>(type: "REAL", nullable: false),
                    WeatherAtKickoff = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeTeamStrengthsAtKickoffJSON = table.Column<string>(type: "TEXT", nullable: false),
                    AwayTeamStrengthsAtKickoffJSON = table.Column<string>(type: "TEXT", nullable: false)
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
                        name: "FK_GameRecords_Stadium_StadiumID",
                        column: x => x.StadiumID,
                        principalTable: "Stadium",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuarterBoxScores");

            migrationBuilder.DropTable(
                name: "TeamDriveRecords");

            migrationBuilder.DropTable(
                name: "TeamGameRecords");

            migrationBuilder.DropTable(
                name: "GameRecords");

            migrationBuilder.DropTable(
                name: "SeasonRecords");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Stadium");
        }
    }
}
