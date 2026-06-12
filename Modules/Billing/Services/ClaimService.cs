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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CareSphere.Data;
using CareSphere.Models;

namespace CareSphere.Modules.Billing.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public ClaimService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<InsuranceClaim> CreateClaimAsync(Guid tenantId, Guid invoiceId, string insuranceProvider, string policyNumber, string memberName, decimal claimedAmount)
        {
            var invoice = await _context.BillingInvoices.FindAsync(invoiceId);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            // Generate unique claim number: CLM-YYYYMMDD-XXXX
            var today = DateTime.UtcNow.Date;
            var prefix = $"CLM-{today:yyyyMMdd}-";
            
            var lastClaim = await _context.InsuranceClaims
                .Where(c => c.ClaimNumber.StartsWith(prefix))
                .OrderByDescending(c => c.ClaimNumber)
                .FirstOrDefaultAsync();

            int seq = 1;
            if (lastClaim != null)
            {
                var parts = lastClaim.ClaimNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSeq))
                {
                    seq = lastSeq + 1;
                }
            }
            string claimNumber = $"{prefix}{seq:D4}";

            var claim = new InsuranceClaim
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoiceId,
                PatientId = invoice.PatientId,
                EncounterId = invoice.EncounterId,
                ClaimNumber = claimNumber,
                InsuranceProvider = insuranceProvider,
                PolicyNumber = policyNumber,
                MemberName = memberName,
                ClaimedAmount = claimedAmount,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _context.InsuranceClaims.Add(claim);

            // Log Claim Status History
            var history = new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClaimId = claim.Id,
                PreviousStatus = "None",
                NewStatus = "Draft",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = "system",
                Remarks = "Claim record created in draft mode."
            };
            _context.ClaimStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            // Log Audit Event
            // TODO: Replace with logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = tenantId,
                UserId = "system",
                Action = "CLAIM_CREATED",
                ResourceType = "InsuranceClaim",
                ResourceId = claim.Id.ToString()
            });

            return claim;
        }

        public async Task<string> GenerateFhirClaimBundleAsync(Guid claimId)
        {
            // FHIR Bundle generation placeholder - connect to HAPI FHIR server when available
            var claim = await _context.InsuranceClaims
                .Include(c => c.Patient)
                .Include(c => c.Encounter)
                .Include(c => c.BillingInvoice)
                    .ThenInclude(i => i.BillingLineItems)
                .FirstOrDefaultAsync(c => c.Id == claimId);

            if (claim == null)
                throw new KeyNotFoundException("Claim not found.");

            // Construct FHIR R4 Claim resource structure
            var fhirClaim = new
            {
                resourceType = "Claim",
                id = claim.Id.ToString(),
                status = "active",
                type = new
                {
                    coding = new[]
                    {
                        new
                        {
                            system = "http://terminology.hl7.org/CodeSystem/claim-type",
                            code = claim.Encounter != null && claim.Encounter.EncounterType == "IPD" ? "institutional" : "professional"
                        }
                    }
                },
                use = "claim",
                patient = new
                {
                    reference = $"Patient/{claim.PatientId}",
                    display = $"{claim.Patient.FirstName} {claim.Patient.LastName}"
                },
                created = claim.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                insurer = new
                {
                    display = claim.InsuranceProvider
                },
                provider = new
                {
                    reference = "Organization/caresphere-hospital",
                    display = "CareSphere Hospital"
                },
                priority = new
                {
                    coding = new[]
                    {
                        new
                        {
                            code = "normal"
                        }
                    }
                },
                insurance = new[]
                {
                    new
                    {
                        sequence = 1,
                        focal = true,
                        coverage = new
                        {
                            identifier = new
                            {
                                value = claim.PolicyNumber
                            }
                        },
                        policyHolder = new
                        {
                            display = claim.MemberName
                        }
                    }
                },
                item = claim.BillingInvoice.BillingLineItems.Select((item, index) => new
                {
                    sequence = index + 1,
                    productOrService = new
                    {
                        coding = new[]
                        {
                            new
                            {
                                code = item.ItemCode ?? "MISC",
                                display = item.ItemDescription
                            }
                        }
                    },
                    quantity = new
                    {
                        value = item.Quantity
                    },
                    unitPrice = new
                    {
                        value = item.UnitPrice,
                        currency = "INR"
                    },
                    net = new
                    {
                        value = item.LineTotal,
                        currency = "INR"
                    }
                }).ToArray(),
                total = new
                {
                    value = claim.ClaimedAmount,
                    currency = "INR"
                }
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string fhirJson = JsonSerializer.Serialize(fhirClaim, options);

            claim.FhirClaimBundleJson = fhirJson;
            claim.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return fhirJson;
        }

        public async Task<InsuranceClaim> SubmitClaimAsync(Guid claimId)
        {
            // Connect to ABDM Claims API when credentials are available
            var claim = await _context.InsuranceClaims.FindAsync(claimId);
            if (claim == null)
                throw new KeyNotFoundException("Claim not found.");

            string prevStatus = claim.Status;
            claim.Status = "Submitted";
            claim.SubmittedAt = DateTime.UtcNow;
            claim.UpdatedAt = DateTime.UtcNow;

            var history = new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                TenantId = claim.TenantId,
                ClaimId = claimId,
                PreviousStatus = prevStatus,
                NewStatus = "Submitted",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = "system",
                Remarks = "Claim submitted to insurance provider."
            };
            _context.ClaimStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            // Log Audit Event
            // TODO: Replace with logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = claim.TenantId,
                UserId = "system",
                Action = "CLAIM_SUBMITTED",
                ResourceType = "InsuranceClaim",
                ResourceId = claimId.ToString()
            });

            return claim;
        }

        public async Task<InsuranceClaim> UpdateClaimStatusAsync(Guid claimId, string newStatus, decimal? approvedAmount, decimal? rejectedAmount, string? rejectionReason, string changedByUserId)
        {
            var claim = await _context.InsuranceClaims.FindAsync(claimId);
            if (claim == null)
                throw new KeyNotFoundException("Claim not found.");

            string prevStatus = claim.Status;
            claim.Status = newStatus;
            claim.UpdatedAt = DateTime.UtcNow;

            if (newStatus == "Approved" || newStatus == "PartiallyApproved")
            {
                claim.ApprovedAmount = approvedAmount;
                claim.ApprovedAt = DateTime.UtcNow;
            }
            else if (newStatus == "Rejected")
            {
                claim.RejectedAmount = rejectedAmount;
                claim.RejectedAt = DateTime.UtcNow;
                claim.RejectionReason = rejectionReason;
            }

            var history = new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                TenantId = claim.TenantId,
                ClaimId = claimId,
                PreviousStatus = prevStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = changedByUserId,
                Remarks = $"Status updated to {newStatus}. Remarks: {rejectionReason}"
            };
            _context.ClaimStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            // Log Audit Event
            // TODO: Replace with logged-in user ID
            await _auditService.LogAsync(new AuditEvent
            {
                TenantId = claim.TenantId,
                UserId = changedByUserId,
                Action = "CLAIM_STATUS_UPDATED",
                ResourceType = "InsuranceClaim",
                ResourceId = claimId.ToString()
            });

            return claim;
        }

        public async Task<List<InsuranceClaim>> GetClaimsByPatientAsync(Guid patientId)
        {
            return await _context.InsuranceClaims
                .Include(c => c.Patient)
                .Include(c => c.ClaimStatusHistories)
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<InsuranceClaim> Claims, int TotalCount)> GetClaimsByStatusAsync(string? status, int page, int pageSize, string searchTerm = "")
        {
            var query = _context.InsuranceClaims
                .Include(c => c.Patient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => 
                    c.ClaimNumber.ToLower().Contains(searchTerm) || 
                    c.Patient.FirstName.ToLower().Contains(searchTerm) || 
                    c.Patient.LastName.ToLower().Contains(searchTerm) ||
                    c.PolicyNumber.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();
            var claims = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (claims, totalCount);
        }

        public async Task<InsuranceClaim?> GetClaimByIdAsync(Guid claimId)
        {
            return await _context.InsuranceClaims
                .Include(c => c.Patient)
                .Include(c => c.Encounter)
                .Include(c => c.BillingInvoice)
                .Include(c => c.ClaimStatusHistories)
                .FirstOrDefaultAsync(c => c.Id == claimId);
        }
    }
}
