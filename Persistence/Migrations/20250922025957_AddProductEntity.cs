using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WeatherForecats",
                table: "WeatherForecats");

            migrationBuilder.RenameTable(
                name: "WeatherForecats",
                newName: "WeatherForecasts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeatherForecasts",
                table: "WeatherForecasts",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WeatherForecasts",
                table: "WeatherForecasts");

            migrationBuilder.RenameTable(
                name: "WeatherForecasts",
                newName: "WeatherForecats");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeatherForecats",
                table: "WeatherForecats",
                column: "Id");
        }
    }
}
