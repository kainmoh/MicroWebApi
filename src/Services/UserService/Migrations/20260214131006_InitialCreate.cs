using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserService.Migrations
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 14, 13, 10, 4, 897, DateTimeKind.Utc).AddTicks(3026), "admin@microservices.com", "System", true, null, "Administrator", "$2a$11$THOBTfzTD4CIqr5kcxtnHOrbVV5ebmUjbLn26pBRpiN07KhDJw6tO", "Admin", "admin" },
                    { 2, new DateTime(2026, 2, 14, 13, 10, 4, 897, DateTimeKind.Utc).AddTicks(3026), "john.doe@example.com", "John", true, null, "Doe", "$2a$11$sZsbtu.YuaeAdGyBuBdNaejUqIhvuoF4ofSK38nA89rGXB9yetsRm", "User", "john.doe" },
                    { 3, new DateTime(2026, 2, 14, 13, 10, 4, 897, DateTimeKind.Utc).AddTicks(3026), "jane.smith@example.com", "Jane", true, null, "Smith", "$2a$11$sZsbtu.YuaeAdGyBuBdNaejUqIhvuoF4ofSK38nA89rGXB9yetsRm", "User", "jane.smith" }
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_UserRole",
                table: "Users",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
