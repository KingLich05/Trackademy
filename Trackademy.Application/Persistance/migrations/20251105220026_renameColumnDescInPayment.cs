using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class renameColumnDescInPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_StudentId_DueDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Payments",
                newName: "PaymentPeriod");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PeriodStart",
                table: "Payments",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PeriodEnd",
                table: "Payments",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId_PeriodEnd",
                table: "Payments",
                columns: new[] { "StudentId", "PeriodEnd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_StudentId_PeriodEnd",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "PaymentPeriod",
                table: "Payments",
                newName: "Description");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodStart",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodEnd",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId_DueDate",
                table: "Payments",
                columns: new[] { "StudentId", "DueDate" });
        }
    }
}
