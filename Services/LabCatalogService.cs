using CareSphere.Data;
using CareSphere.Models;
using Microsoft.EntityFrameworkCore;

namespace CareSphere.Services
{
    public class LabCatalogService : ILabCatalogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public LabCatalogService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<LabTestCatalog> CreateTestAsync(LabTestCatalog test)
        {
            var codeExists = await _context.LabTestCatalogs.AnyAsync(t => 
                t.TenantId == test.TenantId && 
                t.TestCode.ToLower() == test.TestCode.ToLower());

            if (codeExists)
            {
                throw new InvalidOperationException($"A test with code '{test.TestCode}' already exists for this tenant.");
            }

            test.Id = Guid.NewGuid();
            test.CreatedAt = DateTime.UtcNow;

            _context.LabTestCatalogs.Add(test);
            await _context.SaveChangesAsync();

            // Log audit event
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_TEST_CATALOG_CREATED",
                ResourceType = "LabTestCatalog",
                ResourceId = test.Id.ToString(),
                TenantId = test.TenantId
            });

            return test;
        }

        public async Task<LabTestParameter> AddParameterAsync(Guid testId, LabTestParameter parameter)
        {
            var test = await _context.LabTestCatalogs.FindAsync(testId);
            if (test == null)
            {
                throw new KeyNotFoundException("Lab test not found.");
            }

            parameter.Id = Guid.NewGuid();
            parameter.TestId = testId;
            parameter.TenantId = test.TenantId;

            _context.LabTestParameters.Add(parameter);
            await _context.SaveChangesAsync();

            // Log audit event
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_TEST_PARAMETER_ADDED",
                ResourceType = "LabTestParameter",
                ResourceId = parameter.Id.ToString(),
                TenantId = test.TenantId
            });

            return parameter;
        }

        public async Task<LabTestParameter> UpdateParameterAsync(LabTestParameter parameter)
        {
            var existing = await _context.LabTestParameters.FindAsync(parameter.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException("Lab test parameter not found.");
            }

            existing.ParameterName = parameter.ParameterName;
            existing.ParameterCode = parameter.ParameterCode;
            existing.Unit = parameter.Unit;
            existing.ReferenceRangeLow = parameter.ReferenceRangeLow;
            existing.ReferenceRangeHigh = parameter.ReferenceRangeHigh;
            existing.ReferenceRangeText = parameter.ReferenceRangeText;
            existing.DataType = parameter.DataType;
            existing.SortOrder = parameter.SortOrder;

            _context.LabTestParameters.Update(existing);
            await _context.SaveChangesAsync();

            // Log audit event
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_TEST_PARAMETER_UPDATED",
                ResourceType = "LabTestParameter",
                ResourceId = existing.Id.ToString(),
                TenantId = existing.TenantId
            });

            return existing;
        }

        public async Task<LabTestCatalog?> GetTestByIdAsync(Guid id)
        {
            return await _context.LabTestCatalogs
                .Include(t => t.Parameters.OrderBy(p => p.SortOrder))
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<LabTestCatalog>> GetAllTestsAsync(Guid tenantId, string? search, string? category, bool? isActive)
        {
            var query = _context.LabTestCatalogs
                .Include(t => t.Parameters)
                .Where(t => t.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(t => t.TestName.ToLower().Contains(lowerSearch) || t.TestCode.ToLower().Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category == category);
            }

            if (isActive.HasValue)
            {
                query = query.Where(t => t.IsActive == isActive.Value);
            }

            return await query.OrderBy(t => t.TestName).ToListAsync();
        }

        public async Task<List<LabTestCatalog>> GetTestsByCategoryAsync(Guid tenantId, string category)
        {
            return await _context.LabTestCatalogs
                .Where(t => t.TenantId == tenantId && t.IsActive && t.Category == category)
                .OrderBy(t => t.TestName)
                .ToListAsync();
        }
    }
}
