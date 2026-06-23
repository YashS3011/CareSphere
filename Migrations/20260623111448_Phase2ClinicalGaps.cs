using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class Phase2ClinicalGaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "known_allergies",
                table: "patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "discharge_disposition",
                table: "encounters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "triage_priority",
                table: "doctor_queue_entries",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "encounter_diagnoses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    icd_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    icd_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    diagnosis_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encounter_diagnoses", x => x.id);
                    table.ForeignKey(
                        name: "FK_encounter_diagnoses_encounters_encounter_id",
                        column: x => x.encounter_id,
                        principalTable: "encounters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_encounter_diagnoses_encounter_id",
                table: "encounter_diagnoses",
                column: "encounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_EncounterDiagnoses_Tenant_Encounter",
                table: "encounter_diagnoses",
                columns: new[] { "tenant_id", "encounter_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "encounter_diagnoses");

            migrationBuilder.DropColumn(
                name: "known_allergies",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "discharge_disposition",
                table: "encounters");

            migrationBuilder.DropColumn(
                name: "triage_priority",
                table: "doctor_queue_entries");
        }
    }
}
