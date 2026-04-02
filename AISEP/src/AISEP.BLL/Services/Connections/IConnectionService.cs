using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.Connections
{
    public interface IConnectionService
    {
        Task<ConnectionRequestDto> CreateRequestAsync(int investorId, CreateConnectionRequestDto dto);
        Task<ConnectionRequestDto> RespondToRequestAsync(int startupId, int requestId, bool isAccepted);
        Task<ContactInfoDto> GetFounderContactAsync(int investorId, int projectId);
        Task<PagedResult<ConnectionRequestDto>> GetInvestorRequestsAsync(int investorId, SieveModel model, string? status = null);
        Task<PagedResult<ConnectionRequestDto>> GetStartupRequestsAsync(int startupId, SieveModel model, string? status = null);
    }
}
