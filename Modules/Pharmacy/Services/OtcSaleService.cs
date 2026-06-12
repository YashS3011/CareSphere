using CareSphere.Modules.Clinical.Services;
using CareSphere.Modules.Laboratory.Services;
using CareSphere.Modules.Pharmacy.Services;
using CareSphere.Modules.Billing.Services;
using CareSphere.Modules.Patients.Services;
using CareSphere.Modules.Ward.Services;
using CareSphere.Modules.Notifications.Services;
using CareSphere.Modules.Admin.Services;
using CareSphere.Modules.Shared.Services;
using CareSphere.Modules.Shared.Events;
using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.Pharmacy.Services
{
    public class OtcSaleService : IOtcSaleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly RazorpayClientWrapper _razorpayClient;

        public OtcSaleService(ApplicationDbContext context, IAuditService auditService, RazorpayClientWrapper razorpayClient)
        {
            _context = context;
            _auditService = auditService;
            _razorpayClient = razorpayClient;
        }

        public async Task<OtcSale> CreateOtcSaleAsync(OtcSale sale)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                sale.Id = Guid.NewGuid();
                sale.CreatedAt = DateTime.UtcNow;
                sale.SaleDate = DateTime.UtcNow;
                sale.PaymentStatus = "Pending";

                // Generate OTC Sale Number: OTC-YYYYMMDD-XXXX
                var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
                var countToday = await _context.OtcSales
                    .CountAsync(s => s.TenantId == sale.TenantId && s.SaleNumber.StartsWith($"OTC-{todayStr}-"));
                sale.SaleNumber = $"OTC-{todayStr}-{(countToday + 1):D4}";

                var processedItems = new List<OtcSaleItem>();
                decimal totalAmount = 0;

                // Validate items and perform FEFO batch selection / stock allocation
                foreach (var inputItem in sale.Items)
                {
                    var item = await _context.PharmacyItems.FindAsync(inputItem.ItemId);
                    if (item == null)
                        throw new InvalidOperationException($"Pharmacy Item with ID {inputItem.ItemId} not found.");

                    if (item.RequiresPrescription)
                        throw new InvalidOperationException($"Item '{item.ItemName}' requires a prescription and cannot be sold OTC.");

                    if (!item.IsActive)
                        throw new InvalidOperationException($"Item '{item.ItemName}' is currently inactive.");

                    // Query active, non-expired batches order by FEFO (earliest expiry first)
                    var batches = await _context.PharmacyBatches
                        .Where(b => b.ItemId == item.Id && b.IsActive && b.ExpiryDate > DateTime.UtcNow && b.AvailableStock > 0)
                        .OrderBy(b => b.ExpiryDate)
                        .ToListAsync();

                    var available = batches.Sum(b => b.AvailableStock);
                    if (inputItem.Quantity > available)
                        throw new InvalidOperationException($"Insufficient stock for '{item.ItemName}'. Requested: {inputItem.Quantity}, Available: {available}.");

                    var remainingToAllocate = inputItem.Quantity;
                    foreach (var batch in batches)
                    {
                        if (remainingToAllocate <= 0) break;

                        var toAllocate = Math.Min(remainingToAllocate, batch.AvailableStock);
                        
                        if (sale.PaymentMethod == "Cash")
                        {
                            // Deduct stock directly for Cash
                            batch.CurrentStock -= toAllocate;
                            _context.PharmacyBatches.Update(batch);
                        }
                        else
                        {
                            // Reserve stock for online payments (Razorpay)
                            batch.ReservedStock += toAllocate;
                            _context.PharmacyBatches.Update(batch);
                        }

                        var saleItem = new OtcSaleItem
                        {
                            Id = Guid.NewGuid(),
                            TenantId = sale.TenantId,
                            SaleId = sale.Id,
                            ItemId = item.Id,
                            BatchId = batch.Id,
                            BarcodeScanned = inputItem.BarcodeScanned ?? item.Barcode,
                            Quantity = toAllocate,
                            UnitPrice = inputItem.UnitPrice,
                            TotalPrice = toAllocate * inputItem.UnitPrice
                        };

                        processedItems.Add(saleItem);
                        totalAmount += saleItem.TotalPrice;
                        remainingToAllocate -= toAllocate;
                    }
                }

                sale.Items = processedItems;
                sale.TotalAmount = totalAmount;

                if (sale.PaymentMethod == "Cash")
                {
                    sale.PaymentStatus = "Paid";
                }

                _context.OtcSales.Add(sale);
                await _context.SaveChangesAsync();

                // Log audit for Cash completion immediately. online will be done during verification.
                if (sale.PaymentMethod == "Cash")
                {
                    // Write StockLedgerEntries for Cash sale
                    foreach (var saleItem in sale.Items)
                    {
                        var balanceAfter = await _context.PharmacyBatches
                            .Where(b => b.ItemId == saleItem.ItemId && b.IsActive)
                            .SumAsync(b => b.CurrentStock);

                        var ledgerEntry = new StockLedgerEntry
                        {
                            Id = Guid.NewGuid(),
                            TenantId = sale.TenantId,
                            ItemId = saleItem.ItemId,
                            BatchId = saleItem.BatchId,
                            TransactionType = "OtcSale",
                            ReferenceId = sale.SaleNumber,
                            ReferenceType = "OtcSale",
                            QuantityIn = 0,
                            QuantityOut = saleItem.Quantity,
                            BalanceAfter = balanceAfter,
                            TransactionDate = DateTime.UtcNow,
                            CreatedByUserId = sale.CreatedByUserId,
                            Notes = $"OTC Sale: {sale.SaleNumber}"
                        };
                        _context.StockLedgerEntries.Add(ledgerEntry);
                    }

                    await _context.SaveChangesAsync();

                    await _auditService.LogAsync(new AuditEvent
                    {
                        UserId = sale.CreatedByUserId,
                        Action = "OTC_SALE_COMPLETED",
                        ResourceType = "OtcSale",
                        ResourceId = sale.Id.ToString(),
                        TenantId = sale.TenantId
                    });
                }

                await transaction.CommitAsync();
                return sale;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OtcSale> CompleteOtcSaleWithCashAsync(Guid saleId)
        {
            var sale = await _context.OtcSales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null)
                throw new InvalidOperationException("OTC sale not found.");

            if (sale.PaymentStatus == "Paid")
                return sale;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                sale.PaymentMethod = "Cash";
                sale.PaymentStatus = "Paid";

                foreach (var saleItem in sale.Items)
                {
                    var batch = await _context.PharmacyBatches.FindAsync(saleItem.BatchId);
                    if (batch != null)
                    {
                        // Release reserve and deduct from current
                        batch.ReservedStock = Math.Max(0, batch.ReservedStock - saleItem.Quantity);
                        batch.CurrentStock = Math.Max(0, batch.CurrentStock - saleItem.Quantity);
                        _context.PharmacyBatches.Update(batch);
                    }

                    await _context.SaveChangesAsync();

                    var balanceAfter = await _context.PharmacyBatches
                        .Where(b => b.ItemId == saleItem.ItemId && b.IsActive)
                        .SumAsync(b => b.CurrentStock);

                    var ledgerEntry = new StockLedgerEntry
                    {
                        Id = Guid.NewGuid(),
                        TenantId = sale.TenantId,
                        ItemId = saleItem.ItemId,
                        BatchId = saleItem.BatchId,
                        TransactionType = "OtcSale",
                        ReferenceId = sale.SaleNumber,
                        ReferenceType = "OtcSale",
                        QuantityIn = 0,
                        QuantityOut = saleItem.Quantity,
                        BalanceAfter = balanceAfter,
                        TransactionDate = DateTime.UtcNow,
                        CreatedByUserId = sale.CreatedByUserId,
                        Notes = $"OTC Cash Sale: {sale.SaleNumber}"
                    };
                    _context.StockLedgerEntries.Add(ledgerEntry);
                }

                _context.OtcSales.Update(sale);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(new AuditEvent
                {
                    UserId = sale.CreatedByUserId,
                    Action = "OTC_SALE_COMPLETED",
                    ResourceType = "OtcSale",
                    ResourceId = sale.Id.ToString(),
                    TenantId = sale.TenantId
                });

                await transaction.CommitAsync();
                return sale;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RazorpayOrderResult> InitiateRazorpayPaymentAsync(Guid saleId)
        {
            var sale = await _context.OtcSales.FindAsync(saleId);
            if (sale == null)
                throw new InvalidOperationException("OTC sale not found.");

            if (sale.PaymentStatus == "Paid")
                throw new InvalidOperationException("This sale has already been paid.");

            var orderResult = await _razorpayClient.CreateOrderAsync(sale.TotalAmount, sale.SaleNumber);
            
            sale.RazorpayOrderId = orderResult.OrderId;
            sale.PaymentMethod = "Card"; // or Online
            _context.OtcSales.Update(sale);
            await _context.SaveChangesAsync();

            return orderResult;
        }

        public async Task<bool> VerifyRazorpayPaymentAsync(Guid saleId, string razorpayPaymentId, string razorpaySignature)
        {
            var sale = await _context.OtcSales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null)
                throw new InvalidOperationException("OTC sale not found.");

            if (string.IsNullOrEmpty(sale.RazorpayOrderId))
                throw new InvalidOperationException("Razorpay Order ID has not been generated for this sale.");

            var isValid = _razorpayClient.VerifySignature(sale.RazorpayOrderId, razorpayPaymentId, razorpaySignature);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (isValid)
                {
                    sale.PaymentStatus = "Paid";
                    sale.RazorpayPaymentId = razorpayPaymentId;

                    // Finalize stock deduction (release reservation, decrease current stock)
                    foreach (var saleItem in sale.Items)
                    {
                        var batch = await _context.PharmacyBatches.FindAsync(saleItem.BatchId);
                        if (batch != null)
                        {
                            batch.ReservedStock = Math.Max(0, batch.ReservedStock - saleItem.Quantity);
                            batch.CurrentStock = Math.Max(0, batch.CurrentStock - saleItem.Quantity);
                            _context.PharmacyBatches.Update(batch);
                        }

                        await _context.SaveChangesAsync();

                        var balanceAfter = await _context.PharmacyBatches
                            .Where(b => b.ItemId == saleItem.ItemId && b.IsActive)
                            .SumAsync(b => b.CurrentStock);

                        var ledgerEntry = new StockLedgerEntry
                        {
                            Id = Guid.NewGuid(),
                            TenantId = sale.TenantId,
                            ItemId = saleItem.ItemId,
                            BatchId = saleItem.BatchId,
                            TransactionType = "OtcSale",
                            ReferenceId = sale.SaleNumber,
                            ReferenceType = "OtcSale",
                            QuantityIn = 0,
                            QuantityOut = saleItem.Quantity,
                            BalanceAfter = balanceAfter,
                            TransactionDate = DateTime.UtcNow,
                            CreatedByUserId = sale.CreatedByUserId,
                            Notes = $"OTC Razorpay Sale: {sale.SaleNumber}"
                        };
                        _context.StockLedgerEntries.Add(ledgerEntry);
                    }

                    _context.OtcSales.Update(sale);
                    await _context.SaveChangesAsync();

                    await _auditService.LogAsync(new AuditEvent
                    {
                        UserId = sale.CreatedByUserId,
                        Action = "OTC_RAZORPAY_PAYMENT_VERIFIED",
                        ResourceType = "OtcSale",
                        ResourceId = sale.Id.ToString(),
                        TenantId = sale.TenantId
                    });

                    await _auditService.LogAsync(new AuditEvent
                    {
                        UserId = sale.CreatedByUserId,
                        Action = "OTC_SALE_COMPLETED",
                        ResourceType = "OtcSale",
                        ResourceId = sale.Id.ToString(),
                        TenantId = sale.TenantId
                    });

                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    // Payment Verification Failed, release reserved stock
                    sale.PaymentStatus = "Failed";
                    
                    foreach (var saleItem in sale.Items)
                    {
                        var batch = await _context.PharmacyBatches.FindAsync(saleItem.BatchId);
                        if (batch != null)
                        {
                            batch.ReservedStock = Math.Max(0, batch.ReservedStock - saleItem.Quantity);
                            _context.PharmacyBatches.Update(batch);
                        }
                    }

                    _context.OtcSales.Update(sale);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return false;
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OtcSale?> GetOtcSaleByIdAsync(Guid id)
        {
            return await _context.OtcSales
                .Include(s => s.Items)
                    .ThenInclude(si => si.Item)
                .Include(s => s.Items)
                    .ThenInclude(si => si.Batch)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<OtcSale>> GetOtcSalesAsync(Guid tenantId, int page = 1, int pageSize = 10)
        {
            return await _context.OtcSales
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetOtcSalesCountAsync(Guid tenantId)
        {
            return await _context.OtcSales
                .Where(s => s.TenantId == tenantId)
                .CountAsync();
        }
    }
}
