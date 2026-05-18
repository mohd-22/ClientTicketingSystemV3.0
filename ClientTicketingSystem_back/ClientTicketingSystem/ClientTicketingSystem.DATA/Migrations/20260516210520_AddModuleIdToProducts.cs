using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientTicketingSystem.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleIdToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 17, 0, 5, 19, 853, DateTimeKind.Local).AddTicks(5256));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 12, 3, 14, 52, 39, DateTimeKind.Local).AddTicks(8674));
        }
    }
}
