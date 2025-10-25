using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoDataService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Heading = table.Column<int>(type: "integer", nullable: true),
                    Pitch = table.Column<int>(type: "integer", nullable: true),
                    TimesUsed = table.Column<int>(type: "integer", nullable: false),
                    AveragePoints = table.Column<double>(type: "double precision", nullable: true),
                    DifficultyRating = table.Column<int>(type: "integer", nullable: true),
                    Tags = table.Column<List<string>>(type: "jsonb", nullable: false, defaultValue: new List<string>()),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerAId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerBId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerATotalPoints = table.Column<int>(type: "integer", nullable: false),
                    PlayerBTotalPoints = table.Column<int>(type: "integer", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalMatches = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    Draws = table.Column<int>(type: "integer", nullable: false),
                    TotalRoundsPlayed = table.Column<int>(type: "integer", nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false),
                    HighestScore = table.Column<int>(type: "integer", nullable: false),
                    AveragePointsPerRound = table.Column<double>(type: "double precision", nullable: false),
                    TotalDistanceErrorKm = table.Column<double>(type: "double precision", nullable: false),
                    AverageDistanceErrorKm = table.Column<double>(type: "double precision", nullable: false),
                    RankingPoints = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastMatchAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectAnswerLatitude = table.Column<double>(type: "double precision", nullable: false),
                    CorrectAnswerLongitude = table.Column<double>(type: "double precision", nullable: false),
                    PlayerAId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerAGuessLatitude = table.Column<double>(type: "double precision", nullable: true),
                    PlayerAGuessLongitude = table.Column<double>(type: "double precision", nullable: true),
                    PlayerADistance = table.Column<double>(type: "double precision", nullable: true),
                    PlayerAPoints = table.Column<int>(type: "integer", nullable: true),
                    PlayerBId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerBGuessLatitude = table.Column<double>(type: "double precision", nullable: true),
                    PlayerBGuessLongitude = table.Column<double>(type: "double precision", nullable: true),
                    PlayerBDistance = table.Column<double>(type: "double precision", nullable: true),
                    PlayerBPoints = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Country",
                table: "Locations",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_IsActive",
                table: "Locations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Region",
                table: "Locations",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_IsCompleted",
                table: "Matches",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_PlayerAId",
                table: "Matches",
                column: "PlayerAId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_PlayerBId",
                table: "Matches",
                column: "PlayerBId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_StartedAt",
                table: "Matches",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_LastMatchAt",
                table: "PlayerStats",
                column: "LastMatchAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerStats_RankingPoints",
                table: "PlayerStats",
                column: "RankingPoints",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_LocationId",
                table: "Rounds",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_MatchId",
                table: "Rounds",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Matches");
        }
    }
}
