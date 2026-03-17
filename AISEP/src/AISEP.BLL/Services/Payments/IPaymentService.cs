using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Payments
{
    public interface IPaymentService
    {
        Task<IEnumerable<PackageResponse>> GetPackagesAsync();
        Task<CheckoutResponse> CheckoutAsync(int userId, CheckoutRequest request);
        Task<TransactionStatusResponse> GetTransactionStatusAsync(int userId, int transactionId);
        Task ProcessSePayWebhookAsync(SePayWebhookRequest request);
    }
}
