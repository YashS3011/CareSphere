using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class Phase5PharmacyEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_reorder_alert_sent_at",
                table: "pharmacy_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "controlled_substance_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dispense_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dispensed_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    witness_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    log_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_controlled_substance_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_controlled_substance_logs_dispense_records_dispense_record_~",
                        column: x => x.dispense_record_id,
                        principalTable: "dispense_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_controlled_substance_logs_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_controlled_substance_logs_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_controlled_substance_logs_dispense_record_id",
                table: "controlled_substance_logs",
                column: "dispense_record_id");

            migrationBuilder.CreateIndex(
                name: "IX_controlled_substance_logs_item_id",
                table: "controlled_substance_logs",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_controlled_substance_logs_patient_id",
                table: "controlled_substance_logs",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_ControlledSubstanceLogs_Item",
                table: "controlled_substance_logs",
                columns: new[] { "tenant_id", "item_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "controlled_substance_logs");

            migrationBuilder.DropColumn(
                name: "last_reorder_alert_sent_at",
                table: "pharmacy_items");
        }
    }
}
