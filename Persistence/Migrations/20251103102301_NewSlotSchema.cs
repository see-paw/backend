using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewSlotSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Slots_SlotId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Slots_ShelterId_StartDateTime_EndDateTime",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SlotId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "Activities");

            migrationBuilder.RenameColumn(
                name: "SlotStatus",
                table: "Slots",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "ActivityId",
                table: "Slots",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Slots",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Slots",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_ActivityId",
                table: "Slots",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slots_ShelterId",
                table: "Slots",
                column: "ShelterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Activities_ActivityId",
                table: "Slots",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Activities_ActivityId",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Slots_ActivityId",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Slots_ShelterId",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Slots");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Slots",
                newName: "SlotStatus");

            migrationBuilder.AddColumn<string>(
                name: "SlotId",
                table: "Activities",
                type: "character varying(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slots_ShelterId_StartDateTime_EndDateTime",
                table: "Slots",
                columns: new[] { "ShelterId", "StartDateTime", "EndDateTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SlotId",
                table: "Activities",
                column: "SlotId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Slots_SlotId",
                table: "Activities",
                column: "SlotId",
                principalTable: "Slots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
