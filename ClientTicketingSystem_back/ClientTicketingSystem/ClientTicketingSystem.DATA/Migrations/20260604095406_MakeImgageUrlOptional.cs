using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientTicketingSystem.DATA.Migrations
{
    /// <inheritdoc />
    public partial class MakeImgageUrlOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 6, 4, 12, 54, 5, 658, DateTimeKind.Local).AddTicks(878));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 6, 4, 12, 54, 5, 658, DateTimeKind.Local).AddTicks(883));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 6, 4, 12, 54, 5, 658, DateTimeKind.Local).AddTicks(887));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 6, 4, 12, 54, 5, 658, DateTimeKind.Local).AddTicks(637));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4599));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4608));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4615));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4151));
        }
    }
}
