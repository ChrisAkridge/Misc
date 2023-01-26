using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Celarix.JustForFun.FootballSimulator.Data.Migrations
{
    public partial class FixStadiumTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageTemperature",
                table: "Stadium");

            migrationBuilder.RenameColumn(
                name: "OddsOfPrecipitation",
                table: "Stadium",
                newName: "TotalPrecipitationOverSeason");

            migrationBuilder.AddColumn<string>(
                name: "AverageTemperatures",
                table: "Stadium",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageTemperatures",
                table: "Stadium");

            migrationBuilder.RenameColumn(
                name: "TotalPrecipitationOverSeason",
                table: "Stadium",
                newName: "OddsOfPrecipitation");

            migrationBuilder.AddColumn<double>(
                name: "AverageTemperature",
                table: "Stadium",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
