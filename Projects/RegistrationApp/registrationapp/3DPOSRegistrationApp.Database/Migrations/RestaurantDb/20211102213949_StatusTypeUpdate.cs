using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace _3DPOSRegistrationApp.Database.Migrations.RestaurantDb
{
    public partial class StatusTypeUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Staus",
                table: "Item",
                newName: "Status");

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2021, 11, 2, 21, 39, 49, 235, DateTimeKind.Utc).AddTicks(2316));

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2021, 11, 2, 21, 39, 49, 235, DateTimeKind.Utc).AddTicks(2868));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Item",
                newName: "Staus");

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 27, 21, 1, 54, 742, DateTimeKind.Utc).AddTicks(4914));

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 27, 21, 1, 54, 742, DateTimeKind.Utc).AddTicks(5401));
        }
    }
}
