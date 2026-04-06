using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Subscriptions
{
    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDto?> GetMySubscriptionAsync(int userId);
        Task<PagedResult<SubscriptionResponseDto>> GetAllSubscriptionsAsync(SieveModel sieveModel);
    }
}
