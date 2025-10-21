using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrecastTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beds",
                columns: table => new
                {
                    BedId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
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
                name: "ProductionDays",
                columns: table => new
                {
                    ProductionDayId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDays", x => x.ProductionDayId);
                });

            migrationBuilder.CreateTable(
                name: "Pours",
                columns: table => new
                {
                    PourId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                name: "MixDesignRequirements",
                columns: table => new
                {
                    MixDesignRequirementId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestType = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiredPsi = table.Column<int>(type: "INTEGER", nullable: false),
                    MixDesignId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixDesignRequirements", x => x.MixDesignRequirementId);
                    table.ForeignKey(
                        name: "FK_MixDesignRequirements_MixDesigns_MixDesignId",
                        column: x => x.MixDesignId,
                        principalTable: "MixDesigns",
                        principalColumn: "MixDesignId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MixBatches",
                columns: table => new
                {
                    MixBatchId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductionDayId = table.Column<int>(type: "INTEGER", nullable: false),
                    MixDesignId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixBatches", x => x.MixBatchId);
                    table.ForeignKey(
                        name: "FK_MixBatches_MixDesigns_MixDesignId",
                        column: x => x.MixDesignId,
                        principalTable: "MixDesigns",
                        principalColumn: "MixDesignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MixBatches_ProductionDays_ProductionDayId",
                        column: x => x.ProductionDayId,
                        principalTable: "ProductionDays",
                        principalColumn: "ProductionDayId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Placements",
                columns: table => new
                {
                    PlacementId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PieceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Volume = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OvenId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PourId = table.Column<int>(type: "INTEGER", nullable: false),
                    MixBatchId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placements", x => x.PlacementId);
                    table.ForeignKey(
                        name: "FK_Placements_MixBatches_MixBatchId",
                        column: x => x.MixBatchId,
                        principalTable: "MixBatches",
                        principalColumn: "MixBatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Placements_Pours_PourId",
                        column: x => x.PourId,
                        principalTable: "Pours",
                        principalColumn: "PourId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    DeliveryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TruckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PlacementId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.DeliveryId);
                    table.ForeignKey(
                        name: "FK_Deliveries_Placements_PlacementId",
                        column: x => x.PlacementId,
                        principalTable: "Placements",
                        principalColumn: "PlacementId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestSets",
                columns: table => new
                {
                    TestSetId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlacementId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSets", x => x.TestSetId);
                    table.ForeignKey(
                        name: "FK_TestSets_Placements_PlacementId",
                        column: x => x.PlacementId,
                        principalTable: "Placements",
                        principalColumn: "PlacementId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestSetDays",
                columns: table => new
                {
                    TestSetDayId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DayNum = table.Column<int>(type: "INTEGER", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DateDue = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateTested = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TestSetId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSetDays", x => x.TestSetDayId);
                    table.ForeignKey(
                        name: "FK_TestSetDays_TestSets_TestSetId",
                        column: x => x.TestSetId,
                        principalTable: "TestSets",
                        principalColumn: "TestSetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestCylinders",
                columns: table => new
                {
                    TestCylinderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BreakPsi = table.Column<int>(type: "INTEGER", nullable: true),
                    TestSetDayId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCylinders", x => x.TestCylinderId);
                    table.ForeignKey(
                        name: "FK_TestCylinders_TestSetDays_TestSetDayId",
                        column: x => x.TestSetDayId,
                        principalTable: "TestSetDays",
                        principalColumn: "TestSetDayId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_PlacementId",
                table: "Deliveries",
                column: "PlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Code",
                table: "Jobs",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MixBatches_MixDesignId",
                table: "MixBatches",
                column: "MixDesignId");

            migrationBuilder.CreateIndex(
                name: "IX_MixBatches_ProductionDayId",
                table: "MixBatches",
                column: "ProductionDayId");

            migrationBuilder.CreateIndex(
                name: "IX_MixDesignRequirements_MixDesignId_TestType",
                table: "MixDesignRequirements",
                columns: new[] { "MixDesignId", "TestType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MixDesigns_Code",
                table: "MixDesigns",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Placements_MixBatchId",
                table: "Placements",
                column: "MixBatchId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDays_Date",
                table: "ProductionDays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestCylinders_TestSetDayId",
                table: "TestCylinders",
                column: "TestSetDayId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSetDays_TestSetId",
                table: "TestSetDays",
                column: "TestSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSets_PlacementId",
                table: "TestSets",
                column: "PlacementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "MixDesignRequirements");

            migrationBuilder.DropTable(
                name: "TestCylinders");

            migrationBuilder.DropTable(
                name: "TestSetDays");

            migrationBuilder.DropTable(
                name: "TestSets");

            migrationBuilder.DropTable(
                name: "Placements");

            migrationBuilder.DropTable(
                name: "MixBatches");

            migrationBuilder.DropTable(
                name: "Pours");

            migrationBuilder.DropTable(
                name: "MixDesigns");

            migrationBuilder.DropTable(
                name: "ProductionDays");

            migrationBuilder.DropTable(
                name: "Beds");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
