using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAutomationWithFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupStudents_Groups_GroupsId",
                table: "GroupStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStudents_Users_StudentsId",
                table: "GroupStudents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupStudents",
                table: "GroupStudents");

            migrationBuilder.RenameColumn(
                name: "StudentsId",
                table: "GroupStudents",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "GroupsId",
                table: "GroupStudents",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupStudents_StudentsId",
                table: "GroupStudents",
                newName: "IX_GroupStudents_StudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "GroupStudents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "GroupStudents",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DiscountReason",
                table: "GroupStudents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedAt",
                table: "GroupStudents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Генерируем уникальные Id для существующих записей
            migrationBuilder.Sql(@"
                UPDATE ""GroupStudents""
                SET ""Id"" = gen_random_uuid(),
                    ""JoinedAt"" = COALESCE(""JoinedAt"", NOW())
                WHERE ""Id"" = '00000000-0000-0000-0000-000000000000';
            ");

            migrationBuilder.AddColumn<DateTime>(
                name: "CourseEndDate",
                table: "Groups",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "Groups",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupStudents",
                table: "GroupStudents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GroupStudents_GroupId_StudentId",
                table: "GroupStudents",
                columns: new[] { "GroupId", "StudentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStudents_Groups_GroupId",
                table: "GroupStudents",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStudents_Users_StudentId",
                table: "GroupStudents",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupStudents_Groups_GroupId",
                table: "GroupStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStudents_Users_StudentId",
                table: "GroupStudents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupStudents",
                table: "GroupStudents");

            migrationBuilder.DropIndex(
                name: "IX_GroupStudents_GroupId_StudentId",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "DiscountReason",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "CourseEndDate",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "GroupStudents",
                newName: "StudentsId");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "GroupStudents",
                newName: "GroupsId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupStudents_StudentId",
                table: "GroupStudents",
                newName: "IX_GroupStudents_StudentsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupStudents",
                table: "GroupStudents",
                columns: new[] { "GroupsId", "StudentsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStudents_Groups_GroupsId",
                table: "GroupStudents",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStudents_Users_StudentsId",
                table: "GroupStudents",
                column: "StudentsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
