using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;

namespace CareSphere.Infrastructure
{
    public class RazorpayOrderResult
    {
        public string OrderId { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
    }

    public class RazorpayClientWrapper
    {
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayClientWrapper(IConfiguration configuration)
        {
            _keyId = configuration["Razorpay:KeyId"] ?? "rzp_test_placeholder_key";
            _keySecret = configuration["Razorpay:KeySecret"] ?? "placeholder_secret";
        }

        public async Task<RazorpayOrderResult> CreateOrderAsync(decimal amount, string receiptId)
        {
            return await Task.Run(() =>
            {
                var client = new RazorpayClient(_keyId, _keySecret);
                
                // Razorpay expects amount in paise (1 INR = 100 paise)
                long amountInPaise = (long)Math.Round(amount * 100);

                var options = new Dictionary<string, object>
                {
                    { "amount", amountInPaise },
                    { "currency", "INR" },
                    { "receipt", receiptId }
                };

                Order order = client.Order.Create(options);
                string orderId = order["id"].ToString() ?? string.Empty;

                return new RazorpayOrderResult
                {
                    OrderId = orderId,
                    Key = _keyId,
                    Amount = amount,
                    Currency = "INR"
                };
            });
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(signature))
            {
                return false;
            }

            try
            {
                string payload = $"{orderId}|{paymentId}";
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret)))
                {
                    byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                    string expectedSignature = Convert.ToHexString(hash).ToLower();
                    return expectedSignature == signature;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
