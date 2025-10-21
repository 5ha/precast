using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrecastTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsCompleteFromTestSetDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "TestSetDays");

            migrationBuilder.DropColumn(
                name: "DateTested",
                table: "TestCylinders");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDue",
                table: "TestSetDays",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTested",
                table: "TestSetDays",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "TestCylinders",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "Placements",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "BedId",
                table: "Beds",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDue",
                table: "TestSetDays");

            migrationBuilder.DropColumn(
                name: "DateTested",
                table: "TestSetDays");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "TestCylinders");

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "TestSetDays",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTested",
                table: "TestCylinders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "StartTime",
                table: "Placements",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BedId",
                table: "Beds",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
