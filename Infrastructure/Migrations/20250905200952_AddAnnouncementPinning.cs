using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementPinning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Announcements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PinnedAt",
                table: "Announcements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Announcements",
                keyColumn: "Id",
                keyValue: 1,
                column: "PinnedAt",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_IsPinned",
                table: "Announcements",
                column: "IsPinned");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Announcements_IsPinned",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "PinnedAt",
                table: "Announcements");
        }
    }
}
