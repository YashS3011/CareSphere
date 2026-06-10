using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddLaboratoryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "in_app_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recipient_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    resource_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resource_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_in_app_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lab_requisitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requisition_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    encounter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ordered_by_doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    clinical_notes = table.Column<string>(type: "text", nullable: true),
                    fhir_service_request_json = table.Column<string>(type: "text", nullable: true),
                    fhir_service_request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_requisitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_requisitions_doctors_ordered_by_doctor_id",
                        column: x => x.ordered_by_doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lab_requisitions_encounters_encounter_id",
                        column: x => x.encounter_id,
                        principalTable: "encounters",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lab_requisitions_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_test_catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    test_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sample_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    turnaround_hours = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_test_catalog", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lab_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requisition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    generated_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    storage_path = table.Column<string>(type: "text", nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_latest = table.Column<bool>(type: "boolean", nullable: false),
                    notification_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    patient_sms_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    doctor_sms_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    in_app_notification_sent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_reports_lab_requisitions_requisition_id",
                        column: x => x.requisition_id,
                        principalTable: "lab_requisitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_samples",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requisition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sample_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    collected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    collected_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    barcode_label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_received = table.Column<bool>(type: "boolean", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    received_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_samples", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_samples_lab_requisitions_requisition_id",
                        column: x => x.requisition_id,
                        principalTable: "lab_requisitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_requisition_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requisition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    billing_line_item_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_requisition_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_requisition_items_lab_requisitions_requisition_id",
                        column: x => x.requisition_id,
                        principalTable: "lab_requisitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lab_requisition_items_lab_test_catalog_test_id",
                        column: x => x.test_id,
                        principalTable: "lab_test_catalog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_test_parameters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    test_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parameter_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parameter_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reference_range_low = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reference_range_high = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reference_range_text = table.Column<string>(type: "text", nullable: true),
                    data_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_test_parameters", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_test_parameters_lab_test_catalog_test_id",
                        column: x => x.test_id,
                        principalTable: "lab_test_catalog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lab_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requisition_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parameter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    result_value = table.Column<string>(type: "text", nullable: false),
                    result_numeric = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    result_unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reference_range_low = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reference_range_high = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reference_range_text = table.Column<string>(type: "text", nullable: true),
                    is_abnormal = table.Column<bool>(type: "boolean", nullable: false),
                    abnormal_flag = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    entered_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    verified_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    fhir_observation_json = table.Column<string>(type: "text", nullable: true),
                    fhir_observation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_lab_results_lab_requisition_items_requisition_item_id",
                        column: x => x.requisition_item_id,
                        principalTable: "lab_requisition_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lab_results_lab_test_parameters_parameter_id",
                        column: x => x.parameter_id,
                        principalTable: "lab_test_parameters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lab_reports_requisition_id",
                table: "lab_reports",
                column: "requisition_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_requisition_items_requisition_id",
                table: "lab_requisition_items",
                column: "requisition_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_requisition_items_test_id",
                table: "lab_requisition_items",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_requisitions_encounter_id",
                table: "lab_requisitions",
                column: "encounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_requisitions_ordered_by_doctor_id",
                table: "lab_requisitions",
                column: "ordered_by_doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_requisitions_patient_id",
                table: "lab_requisitions",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_requisitions_requisition_number",
                table: "lab_requisitions",
                column: "requisition_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_results_parameter_id",
                table: "lab_results",
                column: "parameter_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_results_requisition_item_id",
                table: "lab_results",
                column: "requisition_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_samples_requisition_id",
                table: "lab_samples",
                column: "requisition_id");

            migrationBuilder.CreateIndex(
                name: "IX_lab_test_catalog_tenant_id_test_code",
                table: "lab_test_catalog",
                columns: new[] { "tenant_id", "test_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_test_parameters_test_id",
                table: "lab_test_parameters",
                column: "test_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "in_app_notifications");

            migrationBuilder.DropTable(
                name: "lab_reports");

            migrationBuilder.DropTable(
                name: "lab_results");

            migrationBuilder.DropTable(
                name: "lab_samples");

            migrationBuilder.DropTable(
                name: "lab_requisition_items");

            migrationBuilder.DropTable(
                name: "lab_test_parameters");

            migrationBuilder.DropTable(
                name: "lab_requisitions");

            migrationBuilder.DropTable(
                name: "lab_test_catalog");
        }
    }
}
