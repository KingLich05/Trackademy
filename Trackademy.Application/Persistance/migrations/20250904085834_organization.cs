using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class organization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUser_Organization_OrganizationsId",
                table: "OrganizationUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organization",
                table: "Organization");

            migrationBuilder.RenameTable(
                name: "Organization",
                newName: "Organizations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUser_Organizations_OrganizationsId",
                table: "OrganizationUser",
                column: "OrganizationsId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUser_Organizations_OrganizationsId",
                table: "OrganizationUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations");

            migrationBuilder.RenameTable(
                name: "Organizations",
                newName: "Organization");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organization",
                table: "Organization",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationUser_Organization_OrganizationsId",
                table: "OrganizationUser",
                column: "OrganizationsId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
