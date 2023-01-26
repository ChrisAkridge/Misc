using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celarix.JustForFun.FootballSimulator.Data.Migrations
{
    public partial class AddSimulatorSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimulatorSettings",
                columns: table => new
                {
                    SimulatorSettingsID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeedDataInitialized = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulatorSettings", x => x.SimulatorSettingsID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimulatorSettings");
        }
    }
}
