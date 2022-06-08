using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace _3DPOSRegistrationApp.Database.Migrations.RestaurantDb
{
    public partial class UpdateSomeField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VariantMasterNymber",
                table: "VariantMaster",
                newName: "VariantMasterNumber");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VariantMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Staus",
                table: "VariantMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Variant",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Staus",
                table: "Variant",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                table: "ItemVariant",
                type: "nvarchar(50)",
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VariantMaster");

            migrationBuilder.DropColumn(
                name: "Staus",
                table: "VariantMaster");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Variant");

            migrationBuilder.DropColumn(
                name: "Staus",
                table: "Variant");

            migrationBuilder.DropColumn(
                name: "VariantName",
                table: "ItemVariant");

            migrationBuilder.RenameColumn(
                name: "VariantMasterNumber",
                table: "VariantMaster",
                newName: "VariantMasterNymber");

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 20, 21, 33, 41, 782, DateTimeKind.Utc).AddTicks(9248));

            migrationBuilder.UpdateData(
                table: "UserStatus",
                keyColumn: "UserStatusId",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2021, 10, 20, 21, 33, 41, 782, DateTimeKind.Utc).AddTicks(9745));
        }
    }
}
