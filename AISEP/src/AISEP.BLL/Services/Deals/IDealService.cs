using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.Deals
{
    public interface IDealService
    {
        Task<DealDto> CreateDealAsync(int investorId, CreateDealDto dto);
        Task<PagedResult<DealDto>> GetInvestorDealsAsync(int investorId, SieveModel sieveModel);
        Task<PagedResult<DealDto>> GetStartupDealsAsync(int startupId, SieveModel sieveModel);
        Task<DealDto> RespondDealAsync(int startupId, int dealId, bool isAccepted);
        Task<string> GetContractPreviewForInvestorAsync(int dealId, int investorId);
        Task<string> GetContractPreviewForStartupAsync(int dealId, int startupId);
        Task<DealContractStatusResponse> InvestorSignContractAsync(int dealId, int investorId, InvestorSignContractDto request);
        Task<DealContractStatusResponse> StartupSignContractAsync(int dealId, int startupId, StartupSignContractDto request);
        Task<DealContractStatusResponse> StartupRejectContractAsync(int dealId, int startupId, StartupRejectContractDto request);
        Task<DealContractStatusResponse> GetContractStatusForInvestorAsync(int dealId, int investorId);
        Task<DealContractStatusResponse> GetContractStatusForStartupAsync(int dealId, int startupId);
        Task<DealDto> MintNftForDealAsync(int dealId, MintNftRequestDto request);
        Task<PagedResult<DealDto>> GetMyNftsAsync(int investorId, SieveModel sieveModel);
    }
}
