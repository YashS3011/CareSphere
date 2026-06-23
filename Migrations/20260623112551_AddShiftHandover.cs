using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftHandover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shift_handovers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    outgoing_nurse_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    incoming_nurse_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    handover_notes = table.Column<string>(type: "text", nullable: false),
                    shift = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    handover_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shift_handovers", x => x.id);
                    table.ForeignKey(
                        name: "FK_shift_handovers_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftHandovers_Patient",
                table: "shift_handovers",
                columns: new[] { "patient_id", "tenant_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shift_handovers");
        }
    }
}
