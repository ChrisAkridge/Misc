using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celarix.JustForFun.FootballSimulator.Data.Migrations
{
    public partial class PlayerTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Player",
                columns: table => new
                {
                    PlayerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Retired = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.PlayerID);
                });

            migrationBuilder.CreateTable(
                name: "PlayerRosterPosition",
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
                    table.PrimaryKey("PK_PlayerRosterPosition", x => x.PlayerRosterPositionID);
                    table.ForeignKey(
                        name: "FK_PlayerRosterPosition_Player_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Player",
                        principalColumn: "PlayerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerRosterPosition_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRosterPosition_PlayerID",
                table: "PlayerRosterPosition",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRosterPosition_TeamID",
                table: "PlayerRosterPosition",
                column: "TeamID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhysicsParams");

            migrationBuilder.DropTable(
                name: "PlayerRosterPosition");

            migrationBuilder.DropTable(
                name: "Player");
        }
    }
}
