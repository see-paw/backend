using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlotId",
                table: "Activities",
                type: "character varying(36)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SlotStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ShelterId = table.Column<string>(type: "character varying(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slots_Shelters_ShelterId",
                        column: x => x.ShelterId,
                        principalTable: "Shelters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SlotId",
                table: "Activities",
                column: "SlotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slots_ShelterId_StartDateTime_EndDateTime",
                table: "Slots",
                columns: new[] { "ShelterId", "StartDateTime", "EndDateTime" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Slots_SlotId",
                table: "Activities",
                column: "SlotId",
                principalTable: "Slots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Slots_SlotId",
                table: "Activities");

            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SlotId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "Activities");
        }
    }
}
