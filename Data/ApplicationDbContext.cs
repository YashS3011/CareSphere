using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CareSphere.Models;
using CareSphere.Infrastructure;

namespace CareSphere.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ITenantContext _tenantContext;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext) : base(options)
        {
            _tenantContext = tenantContext;
        }

        // Existing DbSets
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<BedAllotment> BedAllotments { get; set; }
        public DbSet<BedTransfer> BedTransfers { get; set; }

        // Doctor Workflow & EMR DbSets
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorQueueEntry> DoctorQueueEntries { get; set; }
        public DbSet<Encounter> Encounters { get; set; }
        public DbSet<SoapNote> SoapNotes { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<DrugFormulary> DrugFormulary { get; set; }
        public DbSet<TeleConsultSession> TeleConsultSessions { get; set; }
        public DbSet<DrugInteraction> DrugInteractions { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }

        // Billing & Insurance DbSets
        public DbSet<BillingInvoice> BillingInvoices { get; set; }
        public DbSet<BillingLineItem> BillingLineItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<InsuranceClaim> InsuranceClaims { get; set; }
        public DbSet<ClaimStatusHistory> ClaimStatusHistories { get; set; }
        public DbSet<InvoiceDocument> InvoiceDocuments { get; set; }

        // Pharmacy Management DbSets
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PharmacyItem> PharmacyItems { get; set; }
        public DbSet<PharmacyBatch> PharmacyBatches { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<GoodsReceivedNote> GoodsReceivedNotes { get; set; }
        public DbSet<GrnItem> GrnItems { get; set; }
        public DbSet<StockLedgerEntry> StockLedgerEntries { get; set; }
        public DbSet<DispenseRecord> DispenseRecords { get; set; }
        public DbSet<OtcSale> OtcSales { get; set; }
        public DbSet<OtcSaleItem> OtcSaleItems { get; set; }
        public DbSet<ExpiryAlert> ExpiryAlerts { get; set; }

        // Laboratory Management DbSets
        public DbSet<LabTestCatalog> LabTestCatalogs { get; set; }
        public DbSet<LabTestParameter> LabTestParameters { get; set; }
        public DbSet<LabRequisition> LabRequisitions { get; set; }
        public DbSet<LabRequisitionItem> LabRequisitionItems { get; set; }
        public DbSet<LabSample> LabSamples { get; set; }
        public DbSet<LabResult> LabResults { get; set; }
        public DbSet<LabReport> LabReports { get; set; }
        public DbSet<InAppNotification> InAppNotifications { get; set; }

        // Notifications & Patient Engagement DbSets
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<AppointmentReminder> AppointmentReminders { get; set; }
        public DbSet<DischargeNotification> DischargeNotifications { get; set; }
        public DbSet<PatientPreference> PatientPreferences { get; set; }
        public DbSet<ServiceBusOutbox> ServiceBusOutboxes { get; set; }

        // --- Administration & Access Control DbSets ---
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<TenantSettings> TenantSettings { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Existing entity configurations ---

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasIndex(e => e.Mrn).IsUnique().HasDatabaseName("IX_Patients_MRN");
                entity.HasIndex(e => new { e.TenantId, e.Phone }).HasDatabaseName("IX_Patients_Tenant_Phone");
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<Ward>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<Bed>(entity =>
            {
                entity.HasIndex(e => e.BedNumber).IsUnique();
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<BedAllotment>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_BedAllotments_Tenant_Status");
            });

            modelBuilder.Entity<BedTransfer>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            });

            // --- Doctor Workflow & EMR entity configurations ---

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasIndex(e => e.RegistrationNumber).IsUnique();
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<DoctorQueueEntry>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CheckedInAt).HasDefaultValueSql("now()");
                entity.HasIndex(e => new { e.DoctorId, e.Status }).HasDatabaseName("IX_DoctorQueue_Doctor_Status");
            });

            modelBuilder.Entity<Encounter>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_Encounters_Tenant_Status");
            });

            modelBuilder.Entity<SoapNote>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.IssuedAt).HasDefaultValueSql("now()");
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_Prescriptions_Tenant_Status");
            });

            modelBuilder.Entity<DrugFormulary>(entity =>
            {
                entity.HasIndex(e => e.DrugCode).IsUnique();
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<TeleConsultSession>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<DrugInteraction>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<AuditEvent>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Timestamp).HasDefaultValueSql("now()");
            });

            // --- Billing, Payments, and Insurance Claims configurations ---

            modelBuilder.Entity<BillingInvoice>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.InvoiceDate).HasDefaultValueSql("now()");
                entity.Property(e => e.SubtotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
                entity.Property(e => e.BalanceAmount)
                      .HasComputedColumnSql("\"total_amount\" - \"paid_amount\"", stored: true)
                      .HasPrecision(18, 2);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_BillingInvoices_Tenant_Status");
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("chk_invoice_paid", "\"paid_amount\" >= 0 AND \"paid_amount\" <= \"total_amount\"");
                    t.HasCheckConstraint("chk_balance", "\"balance_amount\" = \"total_amount\" - \"paid_amount\"");
                });
            });

            modelBuilder.Entity<BillingLineItem>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Quantity).HasPrecision(18, 2);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.DiscountPercent).HasPrecision(18, 2);
                entity.Property(e => e.TaxPercent).HasPrecision(18, 2);
                entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("now()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<InsuranceClaim>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.ClaimedAmount).HasPrecision(18, 2);
                entity.Property(e => e.ApprovedAmount).HasPrecision(18, 2);
                entity.Property(e => e.RejectedAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.ClaimNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_InsuranceClaims_Tenant_Status");
            });

            modelBuilder.Entity<ClaimStatusHistory>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.ChangedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<InvoiceDocument>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.GeneratedAt).HasDefaultValueSql("now()");
            });

            // --- Pharmacy Management configurations ---

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<PharmacyItem>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(e => new { e.TenantId, e.ItemCode }).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Barcode })
                      .IsUnique()
                      .HasFilter("\"barcode\" IS NOT NULL");
            });

            modelBuilder.Entity<PharmacyBatch>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
                entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
                entity.Property(e => e.AvailableStock)
                      .HasComputedColumnSql("\"current_stock\" - \"reserved_stock\"", stored: true);
                entity.HasIndex(e => new { e.TenantId, e.IsActive }).HasDatabaseName("IX_PharmacyBatches_Tenant_Status");
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("chk_stock_positive", "\"current_stock\" >= 0 AND \"reserved_stock\" >= 0");
                });
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.PoNumber).IsUnique();
            });

            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<GoodsReceivedNote>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.GrnNumber).IsUnique();
            });

            modelBuilder.Entity<GrnItem>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.PurchasePrice).HasPrecision(18, 2);
                entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<StockLedgerEntry>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.TransactionDate).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<DispenseRecord>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.DispensedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<OtcSale>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.SaleNumber).IsUnique();
            });

            modelBuilder.Entity<OtcSaleItem>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ExpiryAlert>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.SentAt).HasDefaultValueSql("now()");
            });

            // Laboratory Management configurations
            modelBuilder.Entity<LabTestCatalog>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasIndex(e => new { e.TenantId, e.TestCode }).IsUnique();
            });

            modelBuilder.Entity<LabTestParameter>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.ReferenceRangeLow).HasPrecision(18, 2);
                entity.Property(e => e.ReferenceRangeHigh).HasPrecision(18, 2);
            });

            modelBuilder.Entity<LabRequisition>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.OrderedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(e => e.RequisitionNumber).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_LabRequisitions_Tenant_Status");
            });

            modelBuilder.Entity<LabRequisitionItem>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<LabSample>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CollectedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<LabResult>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.EnteredAt).HasDefaultValueSql("now()");
                entity.Property(e => e.ResultNumeric).HasPrecision(18, 2);
                entity.Property(e => e.ReferenceRangeLow).HasPrecision(18, 2);
                entity.Property(e => e.ReferenceRangeHigh).HasPrecision(18, 2);
            });

            modelBuilder.Entity<LabReport>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.GeneratedAt).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<InAppNotification>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });

            // --- Notifications & Patient Engagement Configurations ---
            modelBuilder.Entity<NotificationTemplate>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.Language).HasDefaultValue("en");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => new { e.TenantId, e.TemplateName, e.Channel, e.Language }).IsUnique();
            });

            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.Language).HasDefaultValue("en");
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.RetryCount).HasDefaultValue(0);
                entity.Property(e => e.MaxRetries).HasDefaultValue(3);
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_Notifications_Tenant_Status");
            });

            modelBuilder.Entity<AppointmentReminder>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Language).HasDefaultValue("en");
                entity.Property(e => e.Status).HasDefaultValue("Scheduled");
            });

            modelBuilder.Entity<DischargeNotification>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.Language).HasDefaultValue("en");
                entity.Property(e => e.Status).HasDefaultValue("Pending");
            });

            modelBuilder.Entity<PatientPreference>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.PreferredLanguage).HasDefaultValue("en");
                entity.Property(e => e.PreferredChannel).HasDefaultValue("SMS");
                entity.Property(e => e.OptOutSms).HasDefaultValue(false);
                entity.Property(e => e.OptOutWhatsApp).HasDefaultValue(false);
                entity.Property(e => e.OptOutVoice).HasDefaultValue(false);
                entity.Property(e => e.AllowAppointmentReminders).HasDefaultValue(true);
                entity.Property(e => e.AllowFollowUpReminders).HasDefaultValue(true);
                entity.Property(e => e.AllowDischargeNotifications).HasDefaultValue(true);
                entity.Property(e => e.AllowLabNotifications).HasDefaultValue(true);
                entity.HasIndex(e => new { e.TenantId, e.PatientId }).IsUnique();
            });

            modelBuilder.Entity<ServiceBusOutbox>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.HasIndex(e => new { e.TenantId, e.Status }).HasDatabaseName("IX_Outbox_Status");
            });

            // --- Administration & Access Control configurations ---

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.GrantedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.HasIndex(e => new { e.TenantId, e.RoleName, e.Permission }).IsUnique();
            });

            modelBuilder.Entity<TenantSettings>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.SsoEnabled).HasDefaultValue(false);
                entity.Property(e => e.MaxUsersAllowed).HasDefaultValue(100);
                entity.Property(e => e.SubscriptionTier).HasDefaultValue("Free");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.TenantId).IsUnique();
            });

            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.LastActivityAt).HasDefaultValueSql("now()");
                entity.Property(e => e.IsRevoked).HasDefaultValue(false);
                entity.HasIndex(e => e.SessionToken).IsUnique();
            });

            // AuditEvents table is append-only.
            // Apply the following Supabase RLS policies manually after running EF migrations:
            // CREATE POLICY audit_append_only ON audit_events FOR UPDATE USING (false);
            // CREATE POLICY audit_no_delete ON audit_events FOR DELETE USING (false);
            // See Migrations/AppendOnlyAuditPolicy.sql for the full script.

            // --- Global Multi-Tenant Query Filters ---
            modelBuilder.Entity<Patient>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Ward>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Bed>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<BedAllotment>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<BedTransfer>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Doctor>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<DoctorQueueEntry>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Encounter>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<SoapNote>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Prescription>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<DrugFormulary>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<TeleConsultSession>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<DrugInteraction>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<AuditEvent>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<BillingInvoice>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<BillingLineItem>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Payment>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<InsuranceClaim>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<ClaimStatusHistory>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<InvoiceDocument>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<Supplier>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<PharmacyItem>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<PharmacyBatch>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<PurchaseOrderItem>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<GoodsReceivedNote>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<GrnItem>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<StockLedgerEntry>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<DispenseRecord>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<OtcSale>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<OtcSaleItem>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<ExpiryAlert>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<LabTestCatalog>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<LabTestParameter>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<LabRequisition>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<LabRequisitionItem>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<LabSample>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<LabResult>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<LabReport>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<InAppNotification>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<NotificationTemplate>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<NotificationLog>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<AppointmentReminder>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<DischargeNotification>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<PatientPreference>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<ServiceBusOutbox>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<UserPermission>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<RolePermission>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<TenantSettings>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<UserSession>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
            modelBuilder.Entity<ApplicationUser>().HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
