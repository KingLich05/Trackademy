using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class addStatusInLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "CancelReason",
                table: "Lessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LessonStatus",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelReason",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LessonStatus",
                table: "Lessons");

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
