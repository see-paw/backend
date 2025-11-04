using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewSlotSchemaV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slots_ShelterId",
                table: "Slots");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Slots",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slots_ShelterId_StartDateTime",
                table: "Slots",
                columns: new[] { "ShelterId", "StartDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Slots_StartDateTime_EndDateTime",
                table: "Slots",
                columns: new[] { "StartDateTime", "EndDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slots_ShelterId_StartDateTime",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Slots_StartDateTime_EndDateTime",
                table: "Slots");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Slots",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slots_ShelterId",
                table: "Slots",
                column: "ShelterId");
        }
    }
}
