using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorWorkflowAndEMR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "allergy_notes",
                table: "patients",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "doctors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    registration_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "diagnoses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    icd10_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    diagnosis_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    diagnosis_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    clinical_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    fhir_condition_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnoses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "discharge_summaries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    discharge_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    final_diagnosis = table.Column<string>(type: "text", nullable: false),
                    treatment_summary = table.Column<string>(type: "text", nullable: false),
                    condition_at_discharge = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    followup_instructions = table.Column<string>(type: "text", nullable: true),
                    authorized_by_doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discharge_summaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_discharge_summaries_doctors_authorized_by_doctor_id",
                        column: x => x.authorized_by_doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "encounters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    queue_id = table.Column<Guid>(type: "uuid", nullable: true),
                    bed_allotment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    encounter_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    chief_complaint = table.Column<string>(type: "text", nullable: false),
                    visit_notes = table.Column<string>(type: "text", nullable: true),
                    start_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fhir_encounter_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encounters", x => x.id);
                    table.ForeignKey(
                        name: "FK_encounters_bed_allotments_bed_allotment_id",
                        column: x => x.bed_allotment_id,
                        principalTable: "bed_allotments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_encounters_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_encounters_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "opd_queue",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    queue_date = table.Column<DateOnly>(type: "date", nullable: false),
                    scheduled_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    token_number = table.Column<int>(type: "integer", nullable: false),
                    visit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opd_queue", x => x.id);
                    table.ForeignKey(
                        name: "FK_opd_queue_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_opd_queue_encounters_encounter_id",
                        column: x => x.encounter_id,
                        principalTable: "encounters",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_opd_queue_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prescriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    medicine_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dosage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    route = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    frequency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    duration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    continue_after_discharge = table.Column<bool>(type: "boolean", nullable: false),
                    fhir_medication_request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_prescriptions_encounters_encounter_id",
                        column: x => x.encounter_id,
                        principalTable: "encounters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "procedures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    procedure_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    procedure_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    performed_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    performed_by_doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    outcome = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    fhir_procedure_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedures", x => x.id);
                    table.ForeignKey(
                        name: "FK_procedures_doctors_performed_by_doctor_id",
                        column: x => x.performed_by_doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_procedures_encounters_encounter_id",
                        column: x => x.encounter_id,
                        principalTable: "encounters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "soap_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subjective = table.Column<string>(type: "text", nullable: false),
                    objective_temp = table.Column<decimal>(type: "numeric", nullable: true),
                    objective_bp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    objective_pulse = table.Column<int>(type: "integer", nullable: true),
                    objective_spo2 = table.Column<decimal>(type: "numeric", nullable: true),
                    objective_rr = table.Column<int>(type: "integer", nullable: true),
                    objective_weight = table.Column<decimal>(type: "numeric", nullable: true),
                    objective_height = table.Column<decimal>(type: "numeric", nullable: true),
                    objective_bmi = table.Column<decimal>(type: "numeric", nullable: true),
                    objective_notes = table.Column<string>(type: "text", nullable: true),
                    assessment = table.Column<string>(type: "text", nullable: false),
                    plan = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_soap_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_soap_notes_encounters_encounter_id",
                        column: x => x.encounter_id,
                        principalTable: "encounters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_diagnoses_encounter_id",
                table: "diagnoses",
                column: "encounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_discharge_summaries_authorized_by_doctor_id",
                table: "discharge_summaries",
                column: "authorized_by_doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_discharge_summaries_encounter_id",
                table: "discharge_summaries",
                column: "encounter_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctors_registration_number",
                table: "doctors",
                column: "registration_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_encounters_bed_allotment_id",
                table: "encounters",
                column: "bed_allotment_id");

            migrationBuilder.CreateIndex(
                name: "IX_encounters_doctor_id",
                table: "encounters",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_encounters_patient_id",
                table: "encounters",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_encounters_queue_id",
                table: "encounters",
                column: "queue_id");

            migrationBuilder.CreateIndex(
                name: "IX_opd_queue_doctor_id",
                table: "opd_queue",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_opd_queue_encounter_id",
                table: "opd_queue",
                column: "encounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_opd_queue_patient_id",
                table: "opd_queue",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_encounter_id",
                table: "prescriptions",
                column: "encounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_procedures_encounter_id",
                table: "procedures",
                column: "encounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_procedures_performed_by_doctor_id",
                table: "procedures",
                column: "performed_by_doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_soap_notes_encounter_id",
                table: "soap_notes",
                column: "encounter_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_diagnoses_encounters_encounter_id",
                table: "diagnoses",
                column: "encounter_id",
                principalTable: "encounters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_discharge_summaries_encounters_encounter_id",
                table: "discharge_summaries",
                column: "encounter_id",
                principalTable: "encounters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_encounters_opd_queue_queue_id",
                table: "encounters",
                column: "queue_id",
                principalTable: "opd_queue",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_opd_queue_encounters_encounter_id",
                table: "opd_queue");

            migrationBuilder.DropTable(
                name: "diagnoses");

            migrationBuilder.DropTable(
                name: "discharge_summaries");

            migrationBuilder.DropTable(
                name: "prescriptions");

            migrationBuilder.DropTable(
                name: "procedures");

            migrationBuilder.DropTable(
                name: "soap_notes");

            migrationBuilder.DropTable(
                name: "encounters");

            migrationBuilder.DropTable(
                name: "opd_queue");

            migrationBuilder.DropTable(
                name: "doctors");

            migrationBuilder.DropColumn(
                name: "allergy_notes",
                table: "patients");
        }
    }
}
