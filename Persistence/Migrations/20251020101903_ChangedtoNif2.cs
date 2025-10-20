using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangedtoNif2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nif",
                table: "Shelters",
                newName: "NIF");

            migrationBuilder.RenameIndex(
                name: "IX_Shelters_Nif",
                table: "Shelters",
                newName: "IX_Shelters_NIF");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NIF",
                table: "Shelters",
                newName: "Nif");

            migrationBuilder.RenameIndex(
                name: "IX_Shelters_NIF",
                table: "Shelters",
                newName: "IX_Shelters_Nif");
        }
    }
}
