using CareSphere.Models;
using CareSphere.Infrastructure;

namespace CareSphere.Services
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
