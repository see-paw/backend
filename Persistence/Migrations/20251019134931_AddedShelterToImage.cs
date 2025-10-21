using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedShelterToImage : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "ShelterId",
                table: "Images",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ShelterId",
                table: "Images",
                column: "ShelterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Shelters_ShelterId",
                table: "Images",
                column: "ShelterId",
                principalTable: "Shelters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Shelters_ShelterId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_ShelterId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ShelterId",
                table: "Images");

            migrationBuilder.CreateIndex(
                name: "IX_Shelters_MainImageId",
                table: "Shelters",
                column: "MainImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shelters_Images_MainImageId",
                table: "Shelters",
                column: "MainImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }
    }
}
