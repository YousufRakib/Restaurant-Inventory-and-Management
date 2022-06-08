using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace _3DPOSRegistrationApp.Database.Migrations.RestaurantDb
{
    public partial class StatusFieldUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Staus",
                table: "VarificationCode",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Staus",
                table: "Menu",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Staus",
                table: "ItemCategory",
                newName: "Status");

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2021, 11, 7, 8, 27, 49, 558, DateTimeKind.Utc).AddTicks(426));

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2021, 11, 7, 8, 27, 49, 558, DateTimeKind.Utc).AddTicks(995));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "VarificationCode",
                newName: "Staus");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Menu",
                newName: "Staus");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ItemCategory",
                newName: "Staus");

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
    }
}
