using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6482));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7027));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7202));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7205));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7208));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7210));
        }
    }
}
