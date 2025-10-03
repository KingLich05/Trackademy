using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trackademy.Application.Persistance.migrations
{
    /// <inheritdoc />
    public partial class AddLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Добавляем колонку как NULLABLE (без default '')
            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Users",
                type: "text",
                nullable: true);

            // 2) Заполняем Login для существующих строк на основе Email (до '@'), lower-case.
            //    Разруливаем коллизии суффиксами _2, _3, ...
            migrationBuilder.Sql(@"
WITH base AS (
  SELECT
    ""Id"",
    LOWER(split_part(""Email"", '@', 1)) AS base_login
  FROM ""Users""
),
dedup AS (
  SELECT
    ""Id"",
    CASE
      WHEN COUNT(*) OVER (PARTITION BY base_login) = 1 THEN base_login
      ELSE base_login || '_' || ROW_NUMBER() OVER (PARTITION BY base_login ORDER BY ""Id"")
    END AS new_login
  FROM base
)
UPDATE ""Users"" u
SET ""Login"" = d.new_login
FROM dedup d
WHERE u.""Id"" = d.""Id"" AND u.""Login"" IS NULL;
");

            // На всякий случай нормализуем регистр
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""Login"" = LOWER(""Login"") WHERE ""Login"" IS NOT NULL;");

            // 3) Делаем колонку NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "Login",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            // 4) Идемпотентно создаём УНИКАЛЬНЫЙ индекс на Login
            migrationBuilder.Sql(@"
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_indexes
    WHERE schemaname = 'public' AND indexname = 'IX_Users_Login'
  ) THEN
    CREATE UNIQUE INDEX ""IX_Users_Login"" ON ""Users"" (""Login"");
  END IF;
END
$$;
");

            // 5) (опционально) Идемпотентно создаём НЕуникальный индекс на Email, если вдруг отсутствует.
            //    Если нужен уникальный Email — замени на CREATE UNIQUE INDEX.
            migrationBuilder.Sql(@"
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_indexes
    WHERE schemaname = 'public' AND indexname = 'IX_Users_Email'
  ) THEN
    CREATE INDEX ""IX_Users_Email"" ON ""Users"" (""Email"");
    -- Или так, если Email должен быть уникальным:
    -- CREATE UNIQUE INDEX ""IX_Users_Email"" ON ""Users"" (""Email"");
  END IF;
END
$$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Login",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Login",
                table: "Users");

            // Вернуть уникальность Email при откате (если была нужна)
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }
    }
}