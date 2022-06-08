using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace _3DPOSRegistrationApp.Database.Migrations.RestaurantDb
{
    public partial class AgainUpdateSomeField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Staus",
                table: "VariantMaster",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Staus",
                table: "Variant",
                newName: "Status");

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 23, 19, 0, 9, 478, DateTimeKind.Utc).AddTicks(8952));

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 23, 19, 0, 9, 478, DateTimeKind.Utc).AddTicks(9481));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "VariantMaster",
                newName: "Staus");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Variant",
                newName: "Staus");

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 23, 10, 28, 24, 111, DateTimeKind.Utc).AddTicks(4229));

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 23, 10, 28, 24, 111, DateTimeKind.Utc).AddTicks(4743));
        }
    }
}
