using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentFieldsFromGroupsAndUpdateSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseEndDate",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Groups");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Subjects");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Subjects",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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
        }
    }
}
