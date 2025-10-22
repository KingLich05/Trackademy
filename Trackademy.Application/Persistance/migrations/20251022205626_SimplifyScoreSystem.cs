using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class SimplifyScoreSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LetterValue",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "PassFailValue",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "ScoreType",
                table: "Scores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LetterValue",
                table: "Scores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PassFailValue",
                table: "Scores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoreType",
                table: "Scores",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
