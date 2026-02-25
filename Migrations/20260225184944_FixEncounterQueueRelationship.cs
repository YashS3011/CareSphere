using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class FixEncounterQueueRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_encounters_opd_queue_queue_id",
                table: "encounters");

            migrationBuilder.DropForeignKey(
                name: "FK_opd_queue_encounters_encounter_id",
                table: "opd_queue");

            migrationBuilder.DropIndex(
                name: "IX_opd_queue_encounter_id",
                table: "opd_queue");

            migrationBuilder.DropIndex(
                name: "IX_encounters_queue_id",
                table: "encounters");

            migrationBuilder.DropColumn(
                name: "encounter_id",
                table: "opd_queue");

            migrationBuilder.CreateIndex(
                name: "IX_encounters_queue_id",
                table: "encounters",
                column: "queue_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_encounters_opd_queue_queue_id",
                table: "encounters",
                column: "queue_id",
                principalTable: "opd_queue",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_encounters_opd_queue_queue_id",
                table: "encounters");

            migrationBuilder.DropIndex(
                name: "IX_encounters_queue_id",
                table: "encounters");

            migrationBuilder.AddColumn<Guid>(
                name: "encounter_id",
                table: "opd_queue",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_opd_queue_encounter_id",
                table: "opd_queue",
                column: "encounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_encounters_queue_id",
                table: "encounters",
                column: "queue_id");

            migrationBuilder.AddForeignKey(
                name: "FK_encounters_opd_queue_queue_id",
                table: "encounters",
                column: "queue_id",
                principalTable: "opd_queue",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_opd_queue_encounters_encounter_id",
                table: "opd_queue",
                column: "encounter_id",
                principalTable: "encounters",
                principalColumn: "id");
        }
    }
}
