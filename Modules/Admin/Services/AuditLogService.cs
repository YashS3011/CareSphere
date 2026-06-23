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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CareSphere.Modules.Admin.Services
{
    /// <summary>
    /// Audit log service — extends AuditService with querying and export for admin UI.
    /// The AuditEvents table is append-only. Never call Update or Delete on AuditEvents.
    /// Unit test note: verify RLS policy blocks UPDATE and DELETE on audit_events
    /// at the Supabase level using the AppendOnlyAuditPolicy.sql migration.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditLogService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // IAuditService implementation
        public async Task LogAsync(AuditEvent auditEvent)
        {
            auditEvent.Id = Guid.NewGuid();
            auditEvent.Timestamp = DateTime.UtcNow;
            _context.AuditEvents.Add(auditEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<AuditEvent>> GetAuditLogsAsync(
            Guid tenantId,
            string? userId = null,
            string? action = null,
            string? resourceType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            var query = _context.AuditEvents.AsNoTracking()
                .Where(e => e.TenantId == tenantId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(e => e.UserId == userId);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(e => e.Action == action);

            if (!string.IsNullOrWhiteSpace(resourceType))
                query = query.Where(e => e.ResourceType == resourceType);

            if (dateFrom.HasValue)
                query = query.Where(e => e.Timestamp >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(e => e.Timestamp <= dateTo.Value.AddDays(1));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AuditEvent>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }

        public async Task<List<AuditEvent>> GetAuditLogsByUserAsync(Guid tenantId, string userId)
        {
            return await _context.AuditEvents.AsNoTracking()
                .Where(e => e.TenantId == tenantId && e.UserId == userId)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditEvent>> GetAuditLogsByResourceAsync(Guid tenantId, string resourceType, string resourceId)
        {
            return await _context.AuditEvents.AsNoTracking()
                .Where(e => e.TenantId == tenantId && e.ResourceType == resourceType && e.ResourceId == resourceId)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();
        }

        public async Task<byte[]> ExportAuditLogsAsync(
            Guid tenantId,
            string? userId = null,
            string? action = null,
            string? resourceType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var result = await GetAuditLogsAsync(tenantId, userId, action, resourceType, dateFrom, dateTo, 1, 100000);
            var events = result.Items;

            // Resolve user names in bulk
            var userIds = events.Select(e => e.UserId).Distinct().ToList();
            var userNames = new Dictionary<string, string>();
            foreach (var uid in userIds)
            {
                var u = await _userManager.FindByIdAsync(uid);
                userNames[uid] = u?.FullName ?? uid;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,UserId,UserName,Action,ResourceType,ResourceId,IpAddress,Device,TenantId");

            foreach (var e in events)
            {
                var userName = userNames.TryGetValue(e.UserId, out var n) ? n : e.UserId;
                sb.AppendLine(
                    $"{e.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                    $"{Escape(e.UserId)}," +
                    $"{Escape(userName)}," +
                    $"{Escape(e.Action)}," +
                    $"{Escape(e.ResourceType)}," +
                    $"{Escape(e.ResourceId)}," +
                    $"{Escape(e.IpAddress ?? "")}," +
                    $"{Escape(e.Device ?? "")}," +
                    $"{e.TenantId}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string Escape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        public async Task<List<string>> GetDistinctActionsAsync(Guid tenantId)
        {
            return await _context.AuditEvents.AsNoTracking()
                .Where(e => e.TenantId == tenantId)
                .Select(e => e.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        public async Task<List<string>> GetDistinctResourceTypesAsync(Guid tenantId)
        {
            return await _context.AuditEvents.AsNoTracking()
                .Where(e => e.TenantId == tenantId)
                .Select(e => e.ResourceType)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }
    }
}
