using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "Mobile", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "admin@company.com", "Διαχειριστής", "6900000000", "admin123", "Admin", "admin" },
                    { 2, "clerk@company.com", "Υπάλληλος", "6999999999", "clerk123", "Clerk", "clerk" }
                });

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "Address", "Name" },
                values: new object[,]
                {
                    { 1, "Λεωφ. Αθηνών 123", "Κεντρική Αποθήκη" },
                    { 2, "Οδός Θησέως 45", "Υποκατάστημα Πειραιά" }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "Action", "Code", "ScanCode", "Timestamp", "UserId", "WarehouseId" },
                values: new object[,]
                {
                    { 1, "Input", "PRD001", "SCN1001", new DateTime(2025, 8, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1, 1 },
                    { 2, "Input", "PRD002", "SCN1002", new DateTime(2025, 8, 1, 10, 0, 0, 0, DateTimeKind.Utc), 2, 1 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Code", "Description", "Name", "Price", "Quantity", "TotalValue", "Unit", "WarehouseId" },
                values: new object[,]
                {
                    { 1, "PRD001", "500ml", "Μπουκάλι Νερό", 0.50m, 100, 50m, "pcs", 1 },
                    { 2, "PRD002", "Pack 500", "Χαρτί Α4", 5.00m, 20, 100m, "pcs", 1 },
                    { 3, "PRD003", "Ξύλινο", "Μολύβι HB", 0.25m, 200, 50m, "pcs", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
