using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class AddFreezeFieldsToGroupStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FreezeReason",
                table: "GroupStudents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FrozenFrom",
                table: "GroupStudents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FrozenTo",
                table: "GroupStudents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrozen",
                table: "GroupStudents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreezeReason",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "FrozenFrom",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "FrozenTo",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "IsFrozen",
                table: "GroupStudents");
        }
    }
}
