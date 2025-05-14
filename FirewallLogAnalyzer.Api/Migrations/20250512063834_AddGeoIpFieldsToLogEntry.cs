using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirewallLogAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoIpFieldsToLogEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationGeoCity",
                table: "LogEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationGeoCountry",
                table: "LogEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLatitude",
                table: "LogEntries",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLongitude",
                table: "LogEntries",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceGeoCity",
                table: "LogEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceGeoCountry",
                table: "LogEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SourceLatitude",
                table: "LogEntries",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SourceLongitude",
                table: "LogEntries",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationGeoCity",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "DestinationGeoCountry",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "SourceGeoCity",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "SourceGeoCountry",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "SourceLatitude",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "SourceLongitude",
                table: "LogEntries");
        }
    }
}
