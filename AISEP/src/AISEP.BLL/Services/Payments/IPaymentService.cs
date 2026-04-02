using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.Payments
{
    public interface IPaymentService
    {
        Task<IEnumerable<PackageResponse>> GetInvestorPackagesAsync();
        Task<IEnumerable<PackageResponse>> GetStartupPackagesAsync();
        Task<CheckoutResponse> CheckoutAsync(int userId, CheckoutRequest request);
        Task<CheckoutResponse> CheckoutBookingAsync(int userId, int bookingId);
        Task<TransactionStatusResponse> GetTransactionStatusAsync(int userId, int transactionId);
        Task<BookingPaymentStatusResponse> GetBookingPaymentStatusAsync(int userId, int bookingId);
        Task<PagedResult<BookingPaymentTransactionResponse>> GetBookingPaymentTransactionsAsync(int userId, SieveModel model);
        Task ProcessSePayWebhookAsync(SePayWebhookRequest request);
    }
}
