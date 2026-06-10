using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "wards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "wards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "user_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "user_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "user_permissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "user_permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "user_permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "tenant_settings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "tele_consult_sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "tele_consult_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "tele_consult_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "suppliers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "suppliers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "stock_ledger",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "stock_ledger",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "stock_ledger",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "soap_notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "soap_notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "service_bus_outbox",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "service_bus_outbox",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "role_permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "role_permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "purchase_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "purchase_order_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "purchase_order_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "purchase_order_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "prescriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "prescriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "prescriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "pharmacy_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "pharmacy_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "pharmacy_batches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "pharmacy_batches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "patients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "patient_preferences",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "patient_preferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "patient_preferences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "otc_sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "otc_sales",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "otc_sale_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "otc_sale_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "otc_sale_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "notification_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "notification_templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "notification_logs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "notification_logs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "lab_test_parameters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lab_test_parameters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "lab_test_parameters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lab_test_catalog",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "lab_test_catalog",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "lab_samples",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lab_samples",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "lab_samples",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "lab_results",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lab_results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "lab_results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lab_requisitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "lab_requisition_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lab_requisition_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "lab_requisition_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "lab_reports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lab_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "lab_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "invoice_documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "invoice_documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "invoice_documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "insurance_claims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "grn_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "grn_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "grn_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "goods_received_notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "goods_received_notes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "expiry_alerts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "expiry_alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "expiry_alerts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "encounters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "encounters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "drug_interactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "drug_interactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "drug_interactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "drug_formulary",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "drug_formulary",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "drug_formulary",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "doctors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "doctors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "doctor_queue_entries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "doctor_queue_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "doctor_queue_entries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "dispense_records",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "dispense_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "dispense_records",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "discharge_notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "discharge_notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "claim_status_history",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "claim_status_history",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "claim_status_history",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "billing_line_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "billing_line_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "billing_line_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "billing_invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "beds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "beds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "bed_transfers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "bed_transfers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "bed_transfers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "bed_allotments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "bed_allotments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "appointment_reminders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "appointment_reminders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "appointment_reminders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "wards");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "wards");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "user_permissions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_permissions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user_permissions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "tenant_settings");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "tele_consult_sessions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "tele_consult_sessions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "tele_consult_sessions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "stock_ledger");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "stock_ledger");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "stock_ledger");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "soap_notes");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "soap_notes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "service_bus_outbox");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "service_bus_outbox");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "role_permissions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "role_permissions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "purchase_order_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "purchase_order_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "purchase_order_items");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "prescriptions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "prescriptions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "prescriptions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "pharmacy_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "pharmacy_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "pharmacy_batches");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "pharmacy_batches");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "patient_preferences");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "patient_preferences");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "otc_sales");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "otc_sales");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "otc_sale_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "otc_sale_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "otc_sale_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "notification_templates");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "notification_logs");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "notification_logs");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "lab_test_parameters");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lab_test_parameters");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "lab_test_parameters");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lab_test_catalog");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "lab_test_catalog");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "lab_samples");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lab_samples");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "lab_samples");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "lab_results");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lab_results");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "lab_results");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lab_requisitions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "lab_requisition_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lab_requisition_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "lab_requisition_items");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "lab_reports");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lab_reports");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "lab_reports");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "invoice_documents");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "invoice_documents");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "invoice_documents");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "insurance_claims");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "grn_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "grn_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "grn_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "goods_received_notes");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "goods_received_notes");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "expiry_alerts");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "expiry_alerts");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "expiry_alerts");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "encounters");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "encounters");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "drug_interactions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "drug_interactions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "drug_interactions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "drug_formulary");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "drug_formulary");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "drug_formulary");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "doctor_queue_entries");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "doctor_queue_entries");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "doctor_queue_entries");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "dispense_records");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "dispense_records");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "dispense_records");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "discharge_notifications");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "discharge_notifications");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "claim_status_history");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "claim_status_history");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "claim_status_history");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "billing_line_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "billing_line_items");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "billing_line_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "billing_invoices");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "beds");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "beds");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "bed_transfers");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "bed_transfers");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "bed_transfers");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "bed_allotments");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "bed_allotments");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "appointment_reminders");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "appointment_reminders");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "appointment_reminders");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                table: "patient_preferences",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");
        }
    }
}
