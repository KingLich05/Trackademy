using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class Room_UniqueNamePerOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_Name",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Name_OrganizationId",
                table: "Rooms",
                columns: new[] { "Name", "OrganizationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_Name_OrganizationId",
                table: "Rooms");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Name",
                table: "Rooms",
                column: "Name",
                unique: true);
        }
    }
}
