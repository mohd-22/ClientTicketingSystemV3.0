using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientTicketingSystem.DATA.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_AssignedTo",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "AssignedTo",
                table: "Tickets",
                newName: "AssignedEmpId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssignedTo",
                table: "Tickets",
                newName: "IX_Tickets_AssignedEmpId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 18, 16, 47, 53, 400, DateTimeKind.Local).AddTicks(6341));

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_AssignedEmpId",
                table: "Tickets",
                column: "AssignedEmpId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_AssignedEmpId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "AssignedEmpId",
                table: "Tickets",
                newName: "AssignedTo");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssignedEmpId",
                table: "Tickets",
                newName: "IX_Tickets_AssignedTo");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 17, 0, 5, 19, 853, DateTimeKind.Local).AddTicks(5256));

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_AssignedTo",
                table: "Tickets",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
