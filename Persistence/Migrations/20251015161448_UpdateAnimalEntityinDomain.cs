using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnimalEntityinDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Animals",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "Animals",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Shelters",
                keyColumn: "ShelterId",
                keyValue: "11111111-1111-1111-1111-111111111111",
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 16, 14, 47, 694, DateTimeKind.Utc).AddTicks(1294));

            migrationBuilder.UpdateData(
                table: "Shelters",
                keyColumn: "ShelterId",
                keyValue: "22222222-2222-2222-2222-222222222222",
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 16, 14, 47, 694, DateTimeKind.Utc).AddTicks(1298));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Animals",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "Animals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.UpdateData(
                table: "Shelters",
                keyColumn: "ShelterId",
                keyValue: "11111111-1111-1111-1111-111111111111",
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 16, 30, 51, 277, DateTimeKind.Utc).AddTicks(3184));

            migrationBuilder.UpdateData(
                table: "Shelters",
                keyColumn: "ShelterId",
                keyValue: "22222222-2222-2222-2222-222222222222",
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 16, 30, 51, 277, DateTimeKind.Utc).AddTicks(3198));
        }
    }
}
