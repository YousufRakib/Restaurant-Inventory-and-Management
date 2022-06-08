using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace _3DPOSRegistrationApp.Database.Migrations.RestaurantDb
{
    public partial class AddMenuLo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ChangeItemName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    UpdatedValue = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    ChangedObject = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    RestaurantCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedByName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuLog", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_MenuLog_RestaurantCode",
                table: "MenuLog",
                column: "RestaurantCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuLog");

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
    }
}
