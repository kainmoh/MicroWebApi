using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class SyncProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6482));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6482));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6482));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6482));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6659));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6662));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6665));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6667));
        }
    }
}
