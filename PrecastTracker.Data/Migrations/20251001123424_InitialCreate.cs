using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrecastTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beds",
                columns: table => new
                {
                    BedId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beds", x => x.BedId);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                });

            migrationBuilder.CreateTable(
                name: "MixDesigns",
                columns: table => new
                {
                    MixDesignId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixDesigns", x => x.MixDesignId);
                });

            migrationBuilder.CreateTable(
                name: "Pours",
                columns: table => new
                {
                    PourId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CastingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    BedId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pours", x => x.PourId);
                    table.ForeignKey(
                        name: "FK_Pours_Beds_BedId",
                        column: x => x.BedId,
                        principalTable: "Beds",
                        principalColumn: "BedId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pours_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Placements",
                columns: table => new
                {
                    PlacementId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    YardsPerBed = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    BatchingStartTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    TruckNumbers = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PieceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OvenId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PourId = table.Column<int>(type: "INTEGER", nullable: false),
                    MixDesignId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placements", x => x.PlacementId);
                    table.ForeignKey(
                        name: "FK_Placements_MixDesigns_MixDesignId",
                        column: x => x.MixDesignId,
                        principalTable: "MixDesigns",
                        principalColumn: "MixDesignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Placements_Pours_PourId",
                        column: x => x.PourId,
                        principalTable: "Pours",
                        principalColumn: "PourId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConcreteTests",
                columns: table => new
                {
                    ConcreteTestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CylinderId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TestingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RequiredPsi = table.Column<int>(type: "INTEGER", nullable: false),
                    Break1 = table.Column<int>(type: "INTEGER", nullable: true),
                    Break2 = table.Column<int>(type: "INTEGER", nullable: true),
                    Break3 = table.Column<int>(type: "INTEGER", nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PlacementId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcreteTests", x => x.ConcreteTestId);
                    table.ForeignKey(
                        name: "FK_ConcreteTests_Placements_PlacementId",
                        column: x => x.PlacementId,
                        principalTable: "Placements",
                        principalColumn: "PlacementId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beds_Code",
                table: "Beds",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConcreteTests_PlacementId",
                table: "ConcreteTests",
                column: "PlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Code",
                table: "Jobs",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MixDesigns_Code",
                table: "MixDesigns",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Placements_MixDesignId",
                table: "Placements",
                column: "MixDesignId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_PourId",
                table: "Placements",
                column: "PourId");

            migrationBuilder.CreateIndex(
                name: "IX_Pours_BedId",
                table: "Pours",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_Pours_JobId",
                table: "Pours",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcreteTests");

            migrationBuilder.DropTable(
                name: "Placements");

            migrationBuilder.DropTable(
                name: "MixDesigns");

            migrationBuilder.DropTable(
                name: "Pours");

            migrationBuilder.DropTable(
                name: "Beds");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
