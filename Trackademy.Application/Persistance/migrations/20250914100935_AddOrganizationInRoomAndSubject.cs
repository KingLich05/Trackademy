using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationInRoomAndSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Subjects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Rooms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_OrganizationId",
                table: "Subjects",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_OrganizationId",
                table: "Rooms",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Organizations_OrganizationId",
                table: "Rooms",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Organizations_OrganizationId",
                table: "Subjects",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Organizations_OrganizationId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Organizations_OrganizationId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_OrganizationId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_OrganizationId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Rooms");
        }
    }
}
