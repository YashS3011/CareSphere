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
    public class PharmacyItemService : IPharmacyItemService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public PharmacyItemService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<PharmacyItem> CreateItemAsync(PharmacyItem item)
        {
            // Check ItemCode uniqueness per tenant
            var codeExists = await _context.PharmacyItems.AnyAsync(i => i.TenantId == item.TenantId && i.ItemCode.ToLower() == item.ItemCode.ToLower());
            if (codeExists)
                throw new InvalidOperationException($"Pharmacy Item with code '{item.ItemCode}' already exists for this tenant.");

            // Check Barcode uniqueness per tenant (only if barcode is provided)
            if (!string.IsNullOrWhiteSpace(item.Barcode))
            {
                var barcodeExists = await _context.PharmacyItems.AnyAsync(i => i.TenantId == item.TenantId && i.Barcode.ToLower() == item.Barcode.ToLower());
                if (barcodeExists)
                    throw new InvalidOperationException($"Pharmacy Item with barcode '{item.Barcode}' already exists for this tenant.");
            }

            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
            item.IsActive = true;

            _context.PharmacyItems.Add(item);

            if (item.Category == "Medicine")
            {
                var formularyExists = await _context.DrugFormulary.AnyAsync(f => f.TenantId == item.TenantId && f.DrugCode.ToLower() == item.ItemCode.ToLower());
                if (!formularyExists)
                {
                    var formulary = new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        TenantId = item.TenantId,
                        DrugCode = item.ItemCode,
                        GenericName = item.GenericName ?? item.ItemName,
                        BrandName = item.ItemName,
                        Form = item.Form ?? "Tablet",
                        Strength = item.Strength ?? "N/A",
                        Unit = item.Unit ?? "Tablet",
                        IsControlled = item.IsControlled,
                        IsActive = item.IsActive
                    };
                    _context.DrugFormulary.Add(formulary);
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "PHARMACY_ITEM_CREATED",
                ResourceType = "PharmacyItem",
                ResourceId = item.Id.ToString(),
                TenantId = item.TenantId
            });

            return item;
        }

        public async Task<PharmacyItem?> GetItemByIdAsync(Guid id)
        {
            return await _context.PharmacyItems.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PharmacyItem?> GetItemByBarcodeAsync(Guid tenantId, string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return null;

            return await _context.PharmacyItems.AsNoTracking()
                .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Barcode == barcode && i.IsActive);
        }

        public async Task<List<PharmacyItem>> SearchItemsAsync(Guid tenantId, string? query, string? category, bool? requiresPrescription, int page = 1, int pageSize = 10)
        {
            var dbQuery = _context.PharmacyItems.AsNoTracking().Where(i => i.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lower = query.ToLower();
                dbQuery = dbQuery.Where(i => i.ItemName.ToLower().Contains(lower) || 
                                             i.ItemCode.ToLower().Contains(lower) ||
                                             (i.GenericName != null && i.GenericName.ToLower().Contains(lower)) ||
                                             (i.Barcode != null && i.Barcode.Contains(lower)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                dbQuery = dbQuery.Where(i => i.Category == category);
            }

            if (requiresPrescription.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.RequiresPrescription == requiresPrescription.Value);
            }

            return await dbQuery
                .OrderBy(i => i.ItemName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetItemsCountAsync(Guid tenantId, string? query, string? category, bool? requiresPrescription)
        {
            var dbQuery = _context.PharmacyItems.AsNoTracking().Where(i => i.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lower = query.ToLower();
                dbQuery = dbQuery.Where(i => i.ItemName.ToLower().Contains(lower) || 
                                             i.ItemCode.ToLower().Contains(lower) ||
                                             (i.GenericName != null && i.GenericName.ToLower().Contains(lower)) ||
                                             (i.Barcode != null && i.Barcode.Contains(lower)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                dbQuery = dbQuery.Where(i => i.Category == category);
            }

            if (requiresPrescription.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.RequiresPrescription == requiresPrescription.Value);
            }

            return await dbQuery.CountAsync();
        }

        public async Task<PharmacyItem> UpdateItemAsync(PharmacyItem item)
        {
            var existing = await _context.PharmacyItems.FindAsync(item.Id);
            if (existing == null)
                throw new InvalidOperationException("Pharmacy item not found.");

            // Check ItemCode uniqueness per tenant if changed
            if (existing.ItemCode.ToLower() != item.ItemCode.ToLower())
            {
                var codeExists = await _context.PharmacyItems.AnyAsync(i => i.TenantId == item.TenantId && i.ItemCode.ToLower() == item.ItemCode.ToLower() && i.Id != item.Id);
                if (codeExists)
                    throw new InvalidOperationException($"Pharmacy Item with code '{item.ItemCode}' already exists.");
            }

            // Check Barcode uniqueness per tenant if changed
            if (!string.IsNullOrWhiteSpace(item.Barcode) && existing.Barcode != item.Barcode)
            {
                var barcodeExists = await _context.PharmacyItems.AnyAsync(i => i.TenantId == item.TenantId && i.Barcode.ToLower() == item.Barcode.ToLower() && i.Id != item.Id);
                if (barcodeExists)
                    throw new InvalidOperationException($"Pharmacy Item with barcode '{item.Barcode}' already exists.");
            }

            var oldItemCode = existing.ItemCode;

            existing.ItemCode = item.ItemCode;
            existing.ItemName = item.ItemName;
            existing.GenericName = item.GenericName;
            existing.Category = item.Category;
            existing.Form = item.Form;
            existing.Strength = item.Strength;
            existing.Unit = item.Unit;
            existing.Barcode = item.Barcode;
            existing.IsControlled = item.IsControlled;
            existing.RequiresPrescription = item.RequiresPrescription;
            existing.ReorderLevel = item.ReorderLevel;
            existing.IsActive = item.IsActive;

            _context.PharmacyItems.Update(existing);

            if (item.Category == "Medicine")
            {
                var formulary = await _context.DrugFormulary.FirstOrDefaultAsync(f => f.TenantId == item.TenantId && f.DrugCode.ToLower() == oldItemCode.ToLower());
                if (formulary == null)
                {
                    formulary = await _context.DrugFormulary.FirstOrDefaultAsync(f => f.TenantId == item.TenantId && f.DrugCode.ToLower() == item.ItemCode.ToLower());
                }

                if (formulary == null)
                {
                    formulary = new DrugFormulary
                    {
                        Id = Guid.NewGuid(),
                        TenantId = item.TenantId,
                        DrugCode = item.ItemCode,
                        GenericName = item.GenericName ?? item.ItemName,
                        BrandName = item.ItemName,
                        Form = item.Form ?? "Tablet",
                        Strength = item.Strength ?? "N/A",
                        Unit = item.Unit ?? "Tablet",
                        IsControlled = item.IsControlled,
                        IsActive = item.IsActive
                    };
                    _context.DrugFormulary.Add(formulary);
                }
                else
                {
                    formulary.DrugCode = item.ItemCode;
                    formulary.GenericName = item.GenericName ?? item.ItemName;
                    formulary.BrandName = item.ItemName;
                    formulary.Form = item.Form ?? "Tablet";
                    formulary.Strength = item.Strength ?? "N/A";
                    formulary.Unit = item.Unit ?? "Tablet";
                    formulary.IsControlled = item.IsControlled;
                    formulary.IsActive = item.IsActive;
                    _context.DrugFormulary.Update(formulary);
                }
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "PHARMACY_ITEM_UPDATED",
                ResourceType = "PharmacyItem",
                ResourceId = existing.Id.ToString(),
                TenantId = existing.TenantId
            });

            return existing;
        }

        public async Task<List<PharmacyItemStockDto>> GetLowStockItemsAsync(Guid tenantId)
        {
            var items = await _context.PharmacyItems.AsNoTracking()
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .ToListAsync();

            var result = new List<PharmacyItemStockDto>();

            foreach (var item in items)
            {
                var totalStock = await _context.PharmacyBatches
                    .Where(b => b.ItemId == item.Id && b.IsActive)
                    .SumAsync(b => b.CurrentStock);

                if (totalStock <= item.ReorderLevel)
                {
                    result.Add(new PharmacyItemStockDto
                    {
                        Item = item,
                        TotalStock = totalStock
                    });
                }
            }

            return result;
        }

        public async Task<List<PharmacyItem>> GetActiveItemsAsync(Guid tenantId)
        {
            return await _context.PharmacyItems.AsNoTracking()
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }
    }
}
