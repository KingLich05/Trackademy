using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class AddIsTrialToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTrial",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTrial",
                table: "Users");
        }
    }
}
