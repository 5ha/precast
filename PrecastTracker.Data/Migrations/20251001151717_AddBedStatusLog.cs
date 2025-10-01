using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrecastTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBedStatusLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigurationNotes",
                table: "Beds",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Beds",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Beds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BedStatusLogs",
                columns: table => new
                {
                    BedStatusLogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BedId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    ToStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BedStatusLogs", x => x.BedStatusLogId);
                    table.ForeignKey(
                        name: "FK_BedStatusLogs_Beds_BedId",
                        column: x => x.BedId,
                        principalTable: "Beds",
                        principalColumn: "BedId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BedStatusLogs_BedId",
                table: "BedStatusLogs",
                column: "BedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BedStatusLogs");

            migrationBuilder.DropColumn(
                name: "ConfigurationNotes",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Beds");
        }
    }
}
