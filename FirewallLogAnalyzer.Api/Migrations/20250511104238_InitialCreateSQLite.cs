using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirewallLogAnalyzer.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSQLite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceIP = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    DestinationIP = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    SourcePort = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationPort = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_Action",
                table: "LogEntries",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_DestinationIP",
                table: "LogEntries",
                column: "DestinationIP");

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_SourceIP",
                table: "LogEntries",
                column: "SourceIP");

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_Timestamp",
                table: "LogEntries",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogEntries");
        }
    }
}
