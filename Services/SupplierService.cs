using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public SupplierService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
        {
            supplier.Id = Guid.NewGuid();
            supplier.CreatedAt = DateTime.UtcNow;
            supplier.IsActive = true;

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system", // In a real app, from user state
                Action = "SUPPLIER_CREATED",
                ResourceType = "Supplier",
                ResourceId = supplier.Id.ToString(),
                TenantId = supplier.TenantId
            });

            return supplier;
        }

        public async Task<Supplier?> GetSupplierByIdAsync(Guid id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<List<Supplier>> GetSuppliersAsync(Guid tenantId, string? searchTerm = null)
        {
            var query = _context.Suppliers.Where(s => s.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(s => s.SupplierName.ToLower().Contains(lowerSearch) || 
                                         (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(lowerSearch)) ||
                                         (s.Phone != null && s.Phone.Contains(lowerSearch)));
            }

            return await query.OrderBy(s => s.SupplierName).ToListAsync();
        }

        public async Task<Supplier> UpdateSupplierAsync(Supplier supplier)
        {
            var existing = await _context.Suppliers.FindAsync(supplier.Id);
            if (existing == null)
                throw new InvalidOperationException("Supplier not found.");

            existing.SupplierName = supplier.SupplierName;
            existing.ContactPerson = supplier.ContactPerson;
            existing.Phone = supplier.Phone;
            existing.Email = supplier.Email;
            existing.Address = supplier.Address;
            existing.GstNumber = supplier.GstNumber;
            existing.LicenseNumber = supplier.LicenseNumber;

            _context.Suppliers.Update(existing);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "SUPPLIER_UPDATED",
                ResourceType = "Supplier",
                ResourceId = existing.Id.ToString(),
                TenantId = existing.TenantId
            });

            return existing;
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                throw new InvalidOperationException("Supplier not found.");

            supplier.IsActive = !supplier.IsActive;
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "SUPPLIER_TOGGLED",
                ResourceType = "Supplier",
                ResourceId = supplier.Id.ToString(),
                TenantId = supplier.TenantId
            });
        }
    }
}
