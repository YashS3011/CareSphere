using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareSphere.Models;

namespace CareSphere.Services
{
    public interface IClaimService
    {
        Task<InsuranceClaim> CreateClaimAsync(Guid tenantId, Guid invoiceId, string insuranceProvider, string policyNumber, string memberName, decimal claimedAmount);
        Task<string> GenerateFhirClaimBundleAsync(Guid claimId);
        Task<InsuranceClaim> SubmitClaimAsync(Guid claimId);
        Task<InsuranceClaim> UpdateClaimStatusAsync(Guid claimId, string newStatus, decimal? approvedAmount, decimal? rejectedAmount, string? rejectionReason, string changedByUserId);
        Task<List<InsuranceClaim>> GetClaimsByPatientAsync(Guid patientId);
        Task<(List<InsuranceClaim> Claims, int TotalCount)> GetClaimsByStatusAsync(string? status, int page, int pageSize, string searchTerm = "");
        Task<InsuranceClaim?> GetClaimByIdAsync(Guid claimId);
    }
}
