using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class UserFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 12, 0, 0, 0, DateTimeKind.Utc), "$2a$11$h8qR9mK5YvZ5xGw5yJ5zWO8.5zK7FqN8xJ5yJ5zWO8.5zK7FqN8xJ" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 12, 0, 0, 0, DateTimeKind.Utc), "$2a$11$abc123xyz789def456ghi789jkl012mno345pqr678stu901vwx234" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 12, 0, 0, 0, DateTimeKind.Utc), "$2a$11$abc123xyz789def456ghi789jkl012mno345pqr678stu901vwx234" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 15, 15, 58, 848, DateTimeKind.Utc).AddTicks(5046), "$2a$11$3/QGVD6VG4LVfoxJX9r9xe.EF.Axrfe6/4nXYVS0JkNxy/ZtxKMq." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 15, 15, 58, 848, DateTimeKind.Utc).AddTicks(5046), "$2a$11$BO4LjMZI5BaF1trosVxtwe773a8KJPbfOgNO5Whm74tucQ1Ok7yrm" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 15, 15, 58, 848, DateTimeKind.Utc).AddTicks(5046), "$2a$11$BO4LjMZI5BaF1trosVxtwe773a8KJPbfOgNO5Whm74tucQ1Ok7yrm" });
        }
    }
}
