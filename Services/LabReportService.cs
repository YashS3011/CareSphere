using CareSphere.Data;
using CareSphere.Models;
using CareSphere.Documents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace CareSphere.Services
{
    public class LabReportService : ILabReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAuditService _auditService;
        private readonly ILabNotificationService _notificationService;
        private readonly IServiceBusService _serviceBusService;
        private readonly IConfiguration _configuration;

        public LabReportService(
            ApplicationDbContext context, 
            IWebHostEnvironment webHostEnvironment, 
            IAuditService auditService, 
            ILabNotificationService notificationService,
            IServiceBusService serviceBusService,
            IConfiguration configuration)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _auditService = auditService;
            _notificationService = notificationService;
            _serviceBusService = serviceBusService;
            _configuration = configuration;
        }

        public async Task<LabReport> GenerateReportAsync(Guid tenantId, Guid requisitionId)
        {
            // Load full requisition details
            var requisition = await _context.LabRequisitions
                .Include(r => r.Patient)
                .Include(r => r.OrderedByDoctor)
                .Include(r => r.Items)
                    .ThenInclude(i => i.Test)
                .FirstOrDefaultAsync(r => r.Id == requisitionId);

            if (requisition == null)
            {
                throw new KeyNotFoundException("Requisition not found.");
            }

            // Load all results for the requisition items
            var itemIds = requisition.Items.Select(i => i.Id).ToList();
            var results = await _context.LabResults
                .Include(r => r.Parameter)
                .Where(r => itemIds.Contains(r.RequisitionItemId))
                .ToListAsync();

            // Establish paths
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "documents", "lab-reports");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"LAB-{requisition.RequisitionNumber}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);
            string relativePath = $"documents/lab-reports/{fileName}";

            // Compile PDF
            var pdfDoc = new LabReportDocument(requisition, results);
            pdfDoc.GeneratePdf(fullPath);

            // Mark previous reports for this requisition as IsLatest = false
            var previousReports = await _context.LabReports
                .Where(r => r.RequisitionId == requisitionId && r.IsLatest)
                .ToListAsync();

            foreach (var report in previousReports)
            {
                report.IsLatest = false;
            }

            var labReport = new LabReport
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RequisitionId = requisitionId,
                GeneratedAt = DateTime.UtcNow,
                GeneratedByUserId = "system",
                StoragePath = relativePath,
                FileName = fileName,
                IsLatest = true
            };

            _context.LabReports.Add(labReport);
            await _context.SaveChangesAsync();

            // Enqueue LabReportReady message to Azure Service Bus.
            // Keep the existing direct call as a fallback with a comment saying direct call fallback if Service Bus is not configured.
            var connStr = _configuration["AzureServiceBus:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connStr))
            {
                // direct call fallback if Service Bus is not configured
                await _notificationService.SendReportNotificationsAsync(tenantId, labReport.Id);
            }
            else
            {
                await _serviceBusService.EnqueueMessageAsync("LabReportReady", new { TenantId = tenantId, LabReportId = labReport.Id }, tenantId);
            }

            // Write to AuditEvents
            // TODO: replace 'system' with logged-in user ID once auth is added
            await _auditService.LogAsync(new AuditEvent
            {
                UserId = "system",
                Action = "LAB_REPORT_GENERATED",
                ResourceType = "LabReport",
                ResourceId = labReport.Id.ToString(),
                TenantId = tenantId
            });

            return labReport;
        }

        public async Task<List<LabReport>> GetReportsByRequisitionAsync(Guid requisitionId)
        {
            return await _context.LabReports
                .Where(r => r.RequisitionId == requisitionId)
                .OrderByDescending(r => r.GeneratedAt)
                .ToListAsync();
        }
    }
}
