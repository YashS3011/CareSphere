using CareSphere.Models;

namespace CareSphere.Services
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    /// <summary>
    /// Extends IAuditService with query, export, and filtering capabilities for the Admin UI.
    /// The AuditEvents table is append-only — never call Update or Delete on it.
    /// Unit test note: verify RLS policy blocks UPDATE and DELETE on audit_events
    /// at the Supabase level using the AppendOnlyAuditPolicy.sql migration.
    /// </summary>
    public interface IAuditLogService : IAuditService
    {
        Task<PagedResult<AuditEvent>> GetAuditLogsAsync(
            Guid tenantId,
            string? userId = null,
            string? action = null,
            string? resourceType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageNumber = 1,
            int pageSize = 50);

        Task<List<AuditEvent>> GetAuditLogsByUserAsync(Guid tenantId, string userId);
        Task<List<AuditEvent>> GetAuditLogsByResourceAsync(Guid tenantId, string resourceType, string resourceId);

        /// <summary>
        /// Exports audit logs matching the filters as a CSV byte array for NABH compliance.
        /// Columns: Timestamp, UserId, UserName, Action, ResourceType, ResourceId, IpAddress, Device, TenantId.
        /// </summary>
        Task<byte[]> ExportAuditLogsAsync(
            Guid tenantId,
            string? userId = null,
            string? action = null,
            string? resourceType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null);

        /// <summary>
        /// Returns distinct action names recorded in audit events for this tenant.
        /// Used to dynamically populate filter dropdowns instead of hardcoded lists.
        /// </summary>
        Task<List<string>> GetDistinctActionsAsync(Guid tenantId);

        /// <summary>
        /// Returns distinct resource types recorded in audit events for this tenant.
        /// Used to dynamically populate filter dropdowns instead of hardcoded lists.
        /// </summary>
        Task<List<string>> GetDistinctResourceTypesAsync(Guid tenantId);
    }
}
