using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedImagesInShelters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shelters_Images_MainImageId",
                table: "Shelters");

            migrationBuilder.DropIndex(
                name: "IX_Shelters_MainImageId",
                table: "Shelters");

            migrationBuilder.DropColumn(
                name: "MainImageId",
                table: "Shelters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainImageId",
                table: "Shelters",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shelters_MainImageId",
                table: "Shelters",
                column: "MainImageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shelters_Images_MainImageId",
                table: "Shelters",
                column: "MainImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
