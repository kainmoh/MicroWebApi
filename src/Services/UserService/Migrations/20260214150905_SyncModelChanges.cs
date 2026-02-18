using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 13, 10, 4, 897, DateTimeKind.Utc).AddTicks(3026), "$2a$11$THOBTfzTD4CIqr5kcxtnHOrbVV5ebmUjbLn26pBRpiN07KhDJw6tO" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 13, 10, 4, 897, DateTimeKind.Utc).AddTicks(3026), "$2a$11$sZsbtu.YuaeAdGyBuBdNaejUqIhvuoF4ofSK38nA89rGXB9yetsRm" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 2, 14, 13, 10, 4, 897, DateTimeKind.Utc).AddTicks(3026), "$2a$11$sZsbtu.YuaeAdGyBuBdNaejUqIhvuoF4ofSK38nA89rGXB9yetsRm" });
        }
    }
}
