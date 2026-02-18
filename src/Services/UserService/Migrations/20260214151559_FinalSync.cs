using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class FinalSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 15, 9, 4, 86, DateTimeKind.Utc).AddTicks(2016), "$2a$11$RWXZsoR6vm/spB.g86Ruv.1UNL7pOsgT20yTEcNSKkqjELvqNAeG." });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 15, 9, 4, 86, DateTimeKind.Utc).AddTicks(2016), "$2a$11$KojQlKd3tGVaMs7BuwhfJeRn8oQj/Mr3qGh3OTQ/SlAiicmM369gK" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 15, 9, 4, 86, DateTimeKind.Utc).AddTicks(2016), "$2a$11$KojQlKd3tGVaMs7BuwhfJeRn8oQj/Mr3qGh3OTQ/SlAiicmM369gK" });
        }
    }
}
