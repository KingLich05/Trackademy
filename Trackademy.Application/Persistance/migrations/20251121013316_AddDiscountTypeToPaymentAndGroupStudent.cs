using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class AddDiscountTypeToPaymentAndGroupStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавляем новые колонки
            migrationBuilder.AddColumn<int>(
                name: "DiscountType",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 1); // 1 = Percentage

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "Payments",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountType",
                table: "GroupStudents",
                type: "integer",
                nullable: false,
                defaultValue: 1); // 1 = Percentage

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "GroupStudents",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Мигрируем данные: копируем DiscountPercentage в DiscountValue
            migrationBuilder.Sql(
                @"UPDATE ""Payments"" SET ""DiscountValue"" = ""DiscountPercentage"", ""DiscountType"" = 1");
            
            migrationBuilder.Sql(
                @"UPDATE ""GroupStudents"" SET ""DiscountValue"" = ""DiscountPercentage"", ""DiscountType"" = 1");

            // Удаляем старые колонки
            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "GroupStudents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "GroupStudents");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "GroupStudents");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Payments",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "GroupStudents",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
