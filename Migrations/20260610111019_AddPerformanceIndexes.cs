using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_doctor_queue_entries_doctor_id",
                table: "doctor_queue_entries");

            migrationBuilder.RenameIndex(
                name: "IX_patients_mrn",
                table: "patients",
                newName: "IX_Patients_MRN");

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_Status",
                table: "service_bus_outbox",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Tenant_Status",
                table: "prescriptions",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyBatches_Tenant_Status",
                table: "pharmacy_batches",
                columns: new[] { "tenant_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Tenant_Phone",
                table: "patients",
                columns: new[] { "tenant_id", "phone" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Tenant_Status",
                table: "notification_logs",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_LabRequisitions_Tenant_Status",
                table: "lab_requisitions",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_Tenant_Status",
                table: "insurance_claims",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_Encounters_Tenant_Status",
                table: "encounters",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorQueue_Doctor_Status",
                table: "doctor_queue_entries",
                columns: new[] { "doctor_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingInvoices_Tenant_Status",
                table: "billing_invoices",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_BedAllotments_Tenant_Status",
                table: "bed_allotments",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Outbox_Status",
                table: "service_bus_outbox");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_Tenant_Status",
                table: "prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyBatches_Tenant_Status",
                table: "pharmacy_batches");

            migrationBuilder.DropIndex(
                name: "IX_Patients_Tenant_Phone",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Tenant_Status",
                table: "notification_logs");

            migrationBuilder.DropIndex(
                name: "IX_LabRequisitions_Tenant_Status",
                table: "lab_requisitions");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceClaims_Tenant_Status",
                table: "insurance_claims");

            migrationBuilder.DropIndex(
                name: "IX_Encounters_Tenant_Status",
                table: "encounters");

            migrationBuilder.DropIndex(
                name: "IX_DoctorQueue_Doctor_Status",
                table: "doctor_queue_entries");

            migrationBuilder.DropIndex(
                name: "IX_BillingInvoices_Tenant_Status",
                table: "billing_invoices");

            migrationBuilder.DropIndex(
                name: "IX_BedAllotments_Tenant_Status",
                table: "bed_allotments");

            migrationBuilder.RenameIndex(
                name: "IX_Patients_MRN",
                table: "patients",
                newName: "IX_patients_mrn");

            migrationBuilder.CreateIndex(
                name: "IX_doctor_queue_entries_doctor_id",
                table: "doctor_queue_entries",
                column: "doctor_id");
        }
    }
}
