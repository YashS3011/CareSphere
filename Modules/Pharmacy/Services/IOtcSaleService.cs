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
using CareSphere.Models;
using CareSphere.Infrastructure;

namespace CareSphere.Modules.Pharmacy.Services
{
    public interface IOtcSaleService
    {
        Task<OtcSale> CreateOtcSaleAsync(OtcSale sale);
        Task<OtcSale> CompleteOtcSaleWithCashAsync(Guid saleId);
        Task<RazorpayOrderResult> InitiateRazorpayPaymentAsync(Guid saleId);
        Task<bool> VerifyRazorpayPaymentAsync(Guid saleId, string razorpayPaymentId, string razorpaySignature);
        Task<OtcSale?> GetOtcSaleByIdAsync(Guid id);
        Task<List<OtcSale>> GetOtcSalesAsync(Guid tenantId, int page = 1, int pageSize = 10);
        Task<int> GetOtcSalesCountAsync(Guid tenantId);
    }
}
