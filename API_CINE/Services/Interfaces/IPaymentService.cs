using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
        Task<bool> VerifyPaymentAsync(string transactionId);
        Task<bool> RefundPaymentAsync(string transactionId, decimal amount);
    }
}
