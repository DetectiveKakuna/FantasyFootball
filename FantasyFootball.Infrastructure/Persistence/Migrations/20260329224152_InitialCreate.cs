using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FantasyFootball.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Experts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    CustomWeight = table.Column<double>(type: "REAL", nullable: false, defaultValue: 1.0),
                    DraftAccuracy_OverallRank = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftAccuracy_Qb = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftAccuracy_Rb = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftAccuracy_Wr = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftAccuracy_Te = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftAccuracy_K = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftAccuracy_Dst = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftAccuracy_Idp = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_OverallRank = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_Qb = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_Rb = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_Wr = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_Te = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_K = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_Dst = table.Column<int>(type: "INTEGER", nullable: true),
                    WeeklyAccuracy_Idp = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftRankingsLastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WeeklyRankingsLastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RosRankingsLastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DraftRankings = table.Column<string>(type: "TEXT", nullable: true),
                    WeeklyRankings = table.Column<string>(type: "TEXT", nullable: true),
                    RosRankings = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SleeperId = table.Column<int>(type: "INTEGER", nullable: false),
                    FantasyProsId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<string>(type: "TEXT", nullable: false),
                    FantasyPositions = table.Column<string>(type: "TEXT", nullable: false),
                    Team = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    InjuryStatus = table.Column<string>(type: "TEXT", nullable: true),
                    DepthChartOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    Age = table.Column<int>(type: "INTEGER", nullable: true),
                    YearsExp = table.Column<int>(type: "INTEGER", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Experts_Slug",
                table: "Experts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_FantasyProsId",
                table: "Players",
                column: "FantasyProsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_SleeperId",
                table: "Players",
                column: "SleeperId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Experts");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
