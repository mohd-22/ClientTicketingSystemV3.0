using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClientTicketingSystem.DATA.Migrations
{
    /// <inheritdoc />
    public partial class addProductSeededData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Description", "LastUpdatedBy", "LastUpdatedDate", "Name" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4599), "A complete human resources management solution that helps companies manage employees, attendance, payroll, vacations, and performance tracking.", null, null, "HR Management System" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4608), "A project management system that helps companies manage projects, tasks, milestones, and timelines.", null, null, "Project Management System" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4615), "A customer relationship management system that helps companies manage customer relationships, leads, and opportunities.", null, null, "Customer Relationship Management System" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 23, 4, 37, 21, 659, DateTimeKind.Local).AddTicks(4151));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 16, 1, 44, 336, DateTimeKind.Local).AddTicks(3019));
        }
    }
}
