using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientTicketingSystem.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddLastLoginField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedDate", "LastLogin" },
                values: new object[] { new DateTime(2026, 5, 18, 17, 13, 26, 103, DateTimeKind.Local).AddTicks(7643), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 18, 16, 47, 53, 400, DateTimeKind.Local).AddTicks(6341));
        }
    }
}
