using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class UpdateScoreSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Scores",
                newName: "Version");

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                table: "Scores",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LetterValue",
                table: "Scores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPoints",
                table: "Scores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumericValue",
                table: "Scores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PassFailValue",
                table: "Scores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousVersionId",
                table: "Scores",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoreType",
                table: "Scores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Scores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Scores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Scores",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Scores_AwardedAt",
                table: "Scores",
                column: "AwardedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_PreviousVersionId",
                table: "Scores",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_SubmissionId_Status",
                table: "Scores",
                columns: new[] { "SubmissionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Scores_TeacherId",
                table: "Scores",
                column: "TeacherId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Scores_MaxPoints_Positive",
                table: "Scores",
                sql: "\"MaxPoints\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Scores_NumericValue_Range",
                table: "Scores",
                sql: "\"NumericValue\" IS NULL OR (\"NumericValue\" >= 0 AND \"NumericValue\" <= \"MaxPoints\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Scores_Version_Positive",
                table: "Scores",
                sql: "\"Version\" > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Scores_PreviousVersionId",
                table: "Scores",
                column: "PreviousVersionId",
                principalTable: "Scores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Users_TeacherId",
                table: "Scores",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Scores_PreviousVersionId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Users_TeacherId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Scores_AwardedAt",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Scores_PreviousVersionId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Scores_SubmissionId_Status",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Scores_TeacherId",
                table: "Scores");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Scores_MaxPoints_Positive",
                table: "Scores");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Scores_NumericValue_Range",
                table: "Scores");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Scores_Version_Positive",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "LetterValue",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "MaxPoints",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "NumericValue",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "PassFailValue",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "ScoreType",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Scores");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "Scores",
                newName: "Points");

            migrationBuilder.AlterColumn<string>(
                name: "Feedback",
                table: "Scores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
