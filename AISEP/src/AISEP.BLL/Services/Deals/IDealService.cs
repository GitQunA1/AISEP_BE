using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.Deals
{
    public interface IDealService
    {
        Task<DealDto> CreateDealForInvestorAsync(int investorId, CreateDealDto dto);
        Task<DealDto> CreateDealForStartupAsync(int startupId, CreateDealDto dto);
        Task<PagedResult<DealDto>> GetDealsAsync(SieveModel sieveModel);
        Task<PagedResult<DealDto>> GetInvestorDealsAsync(int investorId, SieveModel sieveModel);
        Task<PagedResult<DealDto>> GetStartupDealsAsync(int startupId, SieveModel sieveModel);
        Task<DealDto> VerifyDealForInvestorAsync(int investorId, int dealId, VerifyDealRequestDto request);
        Task<DealDto> VerifyDealForStartupAsync(int startupId, int dealId, VerifyDealRequestDto request);
        Task<DealDto> StaffReviewDealAsync(int dealId, StaffReviewDealRequestDto request);
        Task<DealDto> ReuploadDealEvidenceForInvestorAsync(int investorId, int dealId, ReuploadDealEvidenceDto request);
        Task<DealDto> ReuploadDealEvidenceForStartupAsync(int startupId, int dealId, ReuploadDealEvidenceDto request);
        Task<DealDto> GetDealByIdAsync(int dealId);
        Task<DealBlockchainVerificationResponse> GetDealOnChainVerificationAsync(int dealId);
    }
}
