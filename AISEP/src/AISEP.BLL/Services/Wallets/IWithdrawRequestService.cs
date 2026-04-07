using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Wallets
{
    public interface IWithdrawRequestService
    {
        Task<WithdrawRequestResponse> CreateWithdrawRequestAsync(int userId, CreateWithdrawRequestDto dto);
        Task<PagedResult<WithdrawRequestResponse>> GetMyWithdrawRequestsAsync(int userId, SieveModel model);
        Task<PagedResult<WithdrawRequestResponse>> GetAllWithdrawRequestsAsync(SieveModel model);
        Task<WithdrawRequestResponse> ApproveWithdrawRequestAsync(int withdrawRequestId, int reviewerId, string? proofImageUrl);
        Task<WithdrawRequestResponse> RejectWithdrawRequestAsync(int withdrawRequestId, int reviewerId, string? reason);
    }
}
