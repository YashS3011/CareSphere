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
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Modules.Pharmacy.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public PurchaseOrderService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder po)
        {
            po.Id = Guid.NewGuid();
            po.CreatedAt = DateTime.UtcNow;
            po.Status = "Draft";

            // Generate PO Number: PO-YYYYMMDD-XXXX
            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var countToday = await _context.PurchaseOrders
                .CountAsync(p => p.TenantId == po.TenantId && p.PoNumber.StartsWith($"PO-{todayStr}-"));
            po.PoNumber = $"PO-{todayStr}-{(countToday + 1):D4}";

            // Calculate totals
            decimal total = 0;
            foreach (var item in po.Items)
            {
                item.Id = Guid.NewGuid();
                item.PoId = po.Id;
                item.TenantId = po.TenantId;
                item.ReceivedQuantity = 0;
                item.TotalPrice = item.RequestedQuantity * item.UnitPrice;
                total += item.TotalPrice;
            }
            po.TotalAmount = total;

            _context.PurchaseOrders.Add(po);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = po.CreatedByUserId,
                Action = "PO_CREATED",
                ResourceType = "PurchaseOrder",
                ResourceId = po.Id.ToString(),
                TenantId = po.TenantId
            });

            return po;
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id)
        {
            return await _context.PurchaseOrders.AsNoTracking()
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(pi => pi.Item)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync(Guid tenantId, string? status, Guid? supplierId, int page = 1, int pageSize = 10)
        {
            var query = _context.PurchaseOrders.AsNoTracking()
                .Include(p => p.Supplier)
                .Where(p => p.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
            {
                query = query.Where(p => p.SupplierId == supplierId.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetPurchaseOrdersCountAsync(Guid tenantId, string? status, Guid? supplierId)
        {
            var query = _context.PurchaseOrders.AsNoTracking().Where(p => p.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
            {
                query = query.Where(p => p.SupplierId == supplierId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<PurchaseOrder> UpdatePurchaseOrderAsync(PurchaseOrder po)
        {
            var existing = await _context.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == po.Id);

            if (existing == null)
                throw new InvalidOperationException("Purchase order not found.");

            if (existing.Status != "Draft")
                throw new InvalidOperationException("Only draft purchase orders can be modified.");

            existing.SupplierId = po.SupplierId;
            existing.ExpectedDeliveryDate = po.ExpectedDeliveryDate;
            existing.Notes = po.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            // Remove items not present in the new list
            var newItemIds = po.Items.Select(i => i.Id).ToList();
            var itemsToRemove = existing.Items.Where(i => !newItemIds.Contains(i.Id)).ToList();
            _context.PurchaseOrderItems.RemoveRange(itemsToRemove);

            // Update/Add items
            decimal total = 0;
            foreach (var inputItem in po.Items)
            {
                inputItem.TotalPrice = inputItem.RequestedQuantity * inputItem.UnitPrice;
                total += inputItem.TotalPrice;

                var existingItem = existing.Items.FirstOrDefault(i => i.Id == inputItem.Id);
                if (existingItem != null)
                {
                    existingItem.ItemId = inputItem.ItemId;
                    existingItem.RequestedQuantity = inputItem.RequestedQuantity;
                    existingItem.UnitPrice = inputItem.UnitPrice;
                    existingItem.TotalPrice = inputItem.TotalPrice;
                    existingItem.Notes = inputItem.Notes;
                }
                else
                {
                    inputItem.Id = Guid.NewGuid();
                    inputItem.PoId = existing.Id;
                    inputItem.TenantId = existing.TenantId;
                    inputItem.ReceivedQuantity = 0;
                    existing.Items.Add(inputItem);
                }
            }

            existing.TotalAmount = total;
            _context.PurchaseOrders.Update(existing);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "PO_UPDATED",
                ResourceType = "PurchaseOrder",
                ResourceId = existing.Id.ToString(),
                TenantId = existing.TenantId
            });

            return existing;
        }

        public async Task<PurchaseOrder> SendPurchaseOrderAsync(Guid poId)
        {
            var po = await _context.PurchaseOrders.FindAsync(poId);
            if (po == null)
                throw new InvalidOperationException("Purchase order not found.");

            if (po.Status != "Draft")
                throw new InvalidOperationException("Only draft purchase orders can be sent.");

            po.Status = "Sent";
            po.OrderDate = DateTime.UtcNow;
            po.UpdatedAt = DateTime.UtcNow;

            _context.PurchaseOrders.Update(po);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "PO_SENT",
                ResourceType = "PurchaseOrder",
                ResourceId = po.Id.ToString(),
                TenantId = po.TenantId
            });

            return po;
        }

        public async Task CancelPurchaseOrderAsync(Guid poId, string reason)
        {
            var po = await _context.PurchaseOrders.FindAsync(poId);
            if (po == null)
                throw new InvalidOperationException("Purchase order not found.");

            if (po.Status == "Received" || po.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot cancel a purchase order with status '{po.Status}'.");

            po.Status = "Cancelled";
            po.Notes = (po.Notes ?? "") + $"\nCancellation Reason: {reason}";
            po.UpdatedAt = DateTime.UtcNow;

            _context.PurchaseOrders.Update(po);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "PO_CANCELLED",
                ResourceType = "PurchaseOrder",
                ResourceId = po.Id.ToString(),
                TenantId = po.TenantId
            });
        }
    }
}
