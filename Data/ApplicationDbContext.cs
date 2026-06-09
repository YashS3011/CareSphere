using Microsoft.EntityFrameworkCore;
using CareSphere.Models;

namespace CareSphere.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Existing entity configurations ---

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasIndex(e => e.Mrn).IsUnique();
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
            });

            modelBuilder.Entity<Encounter>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
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
        }
    }
}
