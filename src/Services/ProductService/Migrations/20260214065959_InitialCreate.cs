using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "Name", "Price", "Quantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Electronics", new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7027), "High-performance gaming laptop with RTX 4080", "Laptop", 1500.00m, 50, null },
                    { 2, "Electronics", new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7202), "Ergonomic wireless mouse with precision tracking", "Wireless Mouse", 25.00m, 200, null },
                    { 3, "Electronics", new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7205), "RGB mechanical keyboard with Cherry MX switches", "Mechanical Keyboard", 100.00m, 150, null },
                    { 4, "Accessories", new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7208), "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader", "USB-C Hub", 45.00m, 300, null },
                    { 5, "Electronics", new DateTime(2026, 2, 14, 6, 59, 58, 679, DateTimeKind.Utc).AddTicks(7210), "1080p HD webcam with auto-focus", "Webcam HD", 75.00m, 120, null }
                });

            migrationBuilder.CreateIndex(
                name: "IDX_ProductCategory",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IDX_ProductName",
                table: "Products",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
