using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BedAllotments_PatientId_Status",
                table: "bed_allotments",
                columns: new[] { "patient_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_DispenseRecords_PrescriptionId",
                table: "dispense_records",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_RequisitionItemId",
                table: "lab_results",
                column: "requisition_item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BedAllotments_PatientId_Status",
                table: "bed_allotments");

            migrationBuilder.DropIndex(
                name: "IX_DispenseRecords_PrescriptionId",
                table: "dispense_records");

            migrationBuilder.DropIndex(
                name: "IX_LabResults_RequisitionItemId",
                table: "lab_results");
        }
    }
}
