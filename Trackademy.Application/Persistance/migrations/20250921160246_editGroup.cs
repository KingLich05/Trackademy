using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class editGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Organizations_OrganizationId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Subjects_SubjectId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupsUser_Groups_GroupsId",
                table: "GroupsUser");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupsUser_Users_StudentsId",
                table: "GroupsUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupsUser",
                table: "GroupsUser");

            migrationBuilder.DropColumn(
                name: "StudentIds",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "GroupsUser",
                newName: "GroupStudents");

            migrationBuilder.RenameIndex(
                name: "IX_GroupsUser_StudentsId",
                table: "GroupStudents",
                newName: "IX_GroupStudents_StudentsId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Groups",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Groups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupStudents",
                table: "GroupStudents",
                columns: new[] { "GroupsId", "StudentsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Organizations_OrganizationId",
                table: "Groups",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Subjects_SubjectId",
                table: "Groups",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Organizations_OrganizationId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Subjects_SubjectId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStudents_Groups_GroupsId",
                table: "GroupStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStudents_Users_StudentsId",
                table: "GroupStudents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupStudents",
                table: "GroupStudents");

            migrationBuilder.RenameTable(
                name: "GroupStudents",
                newName: "GroupsUser");

            migrationBuilder.RenameIndex(
                name: "IX_GroupStudents_StudentsId",
                table: "GroupsUser",
                newName: "IX_GroupsUser_StudentsId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Groups",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Groups",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "StudentIds",
                table: "Groups",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupsUser",
                table: "GroupsUser",
                columns: new[] { "GroupsId", "StudentsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Organizations_OrganizationId",
                table: "Groups",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Subjects_SubjectId",
                table: "Groups",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsUser_Groups_GroupsId",
                table: "GroupsUser",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsUser_Users_StudentsId",
                table: "GroupsUser",
                column: "StudentsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
