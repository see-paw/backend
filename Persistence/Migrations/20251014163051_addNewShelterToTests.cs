using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addNewShelterToTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Shelters",
                keyColumn: "ShelterId",
                keyValue: "11111111-1111-1111-1111-111111111111",
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 16, 30, 51, 277, DateTimeKind.Utc).AddTicks(3184));

            migrationBuilder.InsertData(
                table: "Shelters",
                columns: new[] { "ShelterId", "City", "ClosingTime", "CreatedAt", "MainImageUrl", "NIF", "Name", "OpeningTime", "Phone", "PostalCode", "Street", "UpdatedAt" },
                values: new object[] { "22222222-2222-2222-2222-222222222222", "Porto", new TimeSpan(0, 18, 0, 0, 0), new DateTime(2025, 10, 14, 16, 30, 51, 277, DateTimeKind.Utc).AddTicks(3198), null, "999999999", "Test Shelter 2", new TimeSpan(0, 9, 0, 0, 0), "224589631", "4000-125", "Rua de cima 898", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Shelters",
                keyColumn: "ShelterId",
                keyValue: "22222222-2222-2222-2222-222222222222");

            migrationBuilder.UpdateData(
                table: "Shelters",
                keyColumn: "ShelterId",
                keyValue: "11111111-1111-1111-1111-111111111111",
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 15, 57, 29, 173, DateTimeKind.Utc).AddTicks(2069));
        }
    }
}
