using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class Phase3WardBillingCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "daily_charge_amount",
                table: "beds",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "encounter_id",
                table: "bed_allotments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bed_allotments_encounter_id",
                table: "bed_allotments",
                column: "encounter_id");

            migrationBuilder.AddForeignKey(
                name: "FK_bed_allotments_encounters_encounter_id",
                table: "bed_allotments",
                column: "encounter_id",
                principalTable: "encounters",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bed_allotments_encounters_encounter_id",
                table: "bed_allotments");

            migrationBuilder.DropIndex(
                name: "IX_bed_allotments_encounter_id",
                table: "bed_allotments");

            migrationBuilder.DropColumn(
                name: "daily_charge_amount",
                table: "beds");

            migrationBuilder.DropColumn(
                name: "encounter_id",
                table: "bed_allotments");
        }
    }
}
