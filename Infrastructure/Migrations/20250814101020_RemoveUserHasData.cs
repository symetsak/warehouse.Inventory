using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserHasData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Σβήσε seeded Inventory rows που δείχνουν στους χρήστες 1 και 2
            migrationBuilder.DeleteData(table: "Inventory", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Inventory", keyColumn: "Id", keyValue: 2);

            // 2) Τώρα μπορείς να σβήσεις τους seeded Users
            migrationBuilder.DeleteData(table: "Users", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Users", keyColumn: "Id", keyValue: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // (προαιρετικά) Επαναφορά παλιών χρηστών & inventory αν κάνεις downgrade
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Mobile", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
            { 1, "admin@company.com",  "Διαχειριστής", "6900000000",
              "$2a$11$wI0a8cQm8o2d3c2rN2m6cO4r4iNw1gE8m7V8gq0q1z8C2oWm2hVWe", "Admin", "admin" },
            { 2, "clerk@company.com", "Υπάλληλος",   "6999999999",
              "$2a$11$7tTtKQ4qz2mPZs5zq0m3Me0Qe2v7dXyqNnH2y3b1rTz7oE9xYB7vS", "Employee", "clerk" }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "ScanCode", "Code", "Action", "WarehouseId", "UserId", "Timestamp", "Quantity" },
                values: new object[,]
                {
            { 1, "SCN1001", "PRD001", "Input", 1, 1, DateTime.SpecifyKind(new DateTime(2025, 8, 1, 9, 0, 0), DateTimeKind.Utc), 0 },
            { 2, "SCN1002", "PRD002", "Input", 1, 2, DateTime.SpecifyKind(new DateTime(2025, 8, 1, 10, 0, 0), DateTimeKind.Utc), 0 }
                });
        }
    }
}
