using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddBedManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ward_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "beds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    bed_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bed_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_beds", x => x.id);
                    table.ForeignKey(
                        name: "FK_beds_wards_ward_id",
                        column: x => x.ward_id,
                        principalTable: "wards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bed_allotments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    bed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    discharge_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    admission_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    admitting_doctor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    discharge_notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bed_allotments", x => x.id);
                    table.ForeignKey(
                        name: "FK_bed_allotments_beds_bed_id",
                        column: x => x.bed_id,
                        principalTable: "beds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_allotments_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bed_transfers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    allotment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_bed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_bed_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transfer_reason = table.Column<string>(type: "text", nullable: false),
                    transferred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bed_transfers", x => x.id);
                    table.ForeignKey(
                        name: "FK_bed_transfers_bed_allotments_allotment_id",
                        column: x => x.allotment_id,
                        principalTable: "bed_allotments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfers_beds_from_bed_id",
                        column: x => x.from_bed_id,
                        principalTable: "beds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bed_transfers_beds_to_bed_id",
                        column: x => x.to_bed_id,
                        principalTable: "beds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bed_allotments_bed_id",
                table: "bed_allotments",
                column: "bed_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_allotments_patient_id",
                table: "bed_allotments",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfers_allotment_id",
                table: "bed_transfers",
                column: "allotment_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfers_from_bed_id",
                table: "bed_transfers",
                column: "from_bed_id");

            migrationBuilder.CreateIndex(
                name: "IX_bed_transfers_to_bed_id",
                table: "bed_transfers",
                column: "to_bed_id");

            migrationBuilder.CreateIndex(
                name: "IX_beds_bed_number",
                table: "beds",
                column: "bed_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_beds_ward_id",
                table: "beds",
                column: "ward_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bed_transfers");

            migrationBuilder.DropTable(
                name: "bed_allotments");

            migrationBuilder.DropTable(
                name: "beds");

            migrationBuilder.DropTable(
                name: "wards");
        }
    }
}
