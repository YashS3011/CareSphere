using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "chk_stock_positive",
                table: "pharmacy_batches",
                sql: "\"current_stock\" >= 0 AND \"reserved_stock\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "chk_balance",
                table: "billing_invoices",
                sql: "\"balance_amount\" = \"total_amount\" - \"paid_amount\"");

            migrationBuilder.AddCheckConstraint(
                name: "chk_invoice_paid",
                table: "billing_invoices",
                sql: "\"paid_amount\" >= 0 AND \"paid_amount\" <= \"total_amount\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_stock_positive",
                table: "pharmacy_batches");

            migrationBuilder.DropCheckConstraint(
                name: "chk_balance",
                table: "billing_invoices");

            migrationBuilder.DropCheckConstraint(
                name: "chk_invoice_paid",
                table: "billing_invoices");
        }
    }
}
