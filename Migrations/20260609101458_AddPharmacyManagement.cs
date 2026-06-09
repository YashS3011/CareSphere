using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareSphere.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "otc_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sale_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sale_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    customer_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    razorpay_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    razorpay_payment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otc_sales", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pharmacy_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    generic_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    form = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    strength = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_controlled = table.Column<bool>(type: "boolean", nullable: false),
                    requires_prescription = table.Column<bool>(type: "boolean", nullable: false),
                    reorder_level = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pharmacy_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contact_person = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    gst_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    license_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pharmacy_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    manufacture_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    selling_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    current_stock = table.Column<int>(type: "integer", nullable: false),
                    reserved_stock = table.Column<int>(type: "integer", nullable: false),
                    available_stock = table.Column<int>(type: "integer", nullable: false, computedColumnSql: "\"current_stock\" - \"reserved_stock\"", stored: true),
                    expiry_alert_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pharmacy_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_pharmacy_batches_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pharmacy_batches_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    po_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_orders_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dispense_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prescription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dispensed_quantity = table.Column<int>(type: "integer", nullable: false),
                    dispensed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    dispensed_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    barcode_scanned = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_partial_dispense = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dispense_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_dispense_records_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dispense_records_pharmacy_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "pharmacy_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dispense_records_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dispense_records_prescriptions_prescription_id",
                        column: x => x.prescription_id,
                        principalTable: "prescriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expiry_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    days_until_expiry = table.Column<int>(type: "integer", nullable: false),
                    alert_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    acknowledged_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expiry_alerts", x => x.id);
                    table.ForeignKey(
                        name: "FK_expiry_alerts_pharmacy_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "pharmacy_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expiry_alerts_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "otc_sale_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barcode_scanned = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otc_sale_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_otc_sale_items_otc_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "otc_sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_otc_sale_items_pharmacy_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "pharmacy_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_otc_sale_items_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_ledger",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reference_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    quantity_in = table.Column<int>(type: "integer", nullable: false),
                    quantity_out = table.Column<int>(type: "integer", nullable: false),
                    balance_after = table.Column<int>(type: "integer", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_ledger", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_ledger_pharmacy_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "pharmacy_batches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_ledger_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "goods_received_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grn_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    po_id = table.Column<Guid>(type: "uuid", nullable: true),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    received_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    received_by_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goods_received_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_goods_received_notes_purchase_orders_po_id",
                        column: x => x.po_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_goods_received_notes_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    po_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_quantity = table.Column<int>(type: "integer", nullable: false),
                    received_quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_purchase_orders_po_id",
                        column: x => x.po_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grn_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grn_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    manufacture_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_quantity = table.Column<int>(type: "integer", nullable: false),
                    free_quantity = table.Column<int>(type: "integer", nullable: false),
                    purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    selling_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grn_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_grn_items_goods_received_notes_grn_id",
                        column: x => x.grn_id,
                        principalTable: "goods_received_notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grn_items_pharmacy_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "pharmacy_batches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_grn_items_pharmacy_items_item_id",
                        column: x => x.item_id,
                        principalTable: "pharmacy_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dispense_records_batch_id",
                table: "dispense_records",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispense_records_item_id",
                table: "dispense_records",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispense_records_patient_id",
                table: "dispense_records",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_dispense_records_prescription_id",
                table: "dispense_records",
                column: "prescription_id");

            migrationBuilder.CreateIndex(
                name: "IX_expiry_alerts_batch_id",
                table: "expiry_alerts",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_expiry_alerts_item_id",
                table: "expiry_alerts",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_goods_received_notes_grn_number",
                table: "goods_received_notes",
                column: "grn_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_goods_received_notes_po_id",
                table: "goods_received_notes",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "IX_goods_received_notes_supplier_id",
                table: "goods_received_notes",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_items_batch_id",
                table: "grn_items",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_items_grn_id",
                table: "grn_items",
                column: "grn_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_items_item_id",
                table: "grn_items",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_otc_sale_items_batch_id",
                table: "otc_sale_items",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_otc_sale_items_item_id",
                table: "otc_sale_items",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_otc_sale_items_sale_id",
                table: "otc_sale_items",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "IX_otc_sales_sale_number",
                table: "otc_sales",
                column: "sale_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pharmacy_batches_item_id",
                table: "pharmacy_batches",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacy_batches_supplier_id",
                table: "pharmacy_batches",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacy_items_tenant_id_barcode",
                table: "pharmacy_items",
                columns: new[] { "tenant_id", "barcode" },
                unique: true,
                filter: "\"barcode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pharmacy_items_tenant_id_item_code",
                table: "pharmacy_items",
                columns: new[] { "tenant_id", "item_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_item_id",
                table: "purchase_order_items",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_po_id",
                table: "purchase_order_items",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_po_number",
                table: "purchase_orders",
                column: "po_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_supplier_id",
                table: "purchase_orders",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_batch_id",
                table: "stock_ledger",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_ledger_item_id",
                table: "stock_ledger",
                column: "item_id");

            // FR-026: Create dispensable_prescriptions view for database-layer authorization enforcement
            // When Supabase RLS is fully configured, add RLS policy on this view so
            // pharmacy role can only read prescriptions belonging to their tenant.
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW dispensable_prescriptions AS
                SELECT p.* FROM prescriptions p
                WHERE p.status = 'Active' AND p.tenant_id IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the dispensable_prescriptions view first
            migrationBuilder.Sql("DROP VIEW IF EXISTS dispensable_prescriptions;");

            migrationBuilder.DropTable(
                name: "dispense_records");

            migrationBuilder.DropTable(
                name: "expiry_alerts");

            migrationBuilder.DropTable(
                name: "grn_items");

            migrationBuilder.DropTable(
                name: "otc_sale_items");

            migrationBuilder.DropTable(
                name: "purchase_order_items");

            migrationBuilder.DropTable(
                name: "stock_ledger");

            migrationBuilder.DropTable(
                name: "goods_received_notes");

            migrationBuilder.DropTable(
                name: "otc_sales");

            migrationBuilder.DropTable(
                name: "pharmacy_batches");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropTable(
                name: "pharmacy_items");

            migrationBuilder.DropTable(
                name: "suppliers");
        }
    }
}
