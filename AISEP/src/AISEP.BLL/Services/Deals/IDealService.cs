using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.Deals
{
    public interface IDealService
    {
        Task<DealDto> CreateDealAsync(int investorId, CreateDealDto dto);
        Task<DealDto> ConfirmDealAsync(int startupId, int dealId);
        Task<DealDto> MintNftForDealAsync(int dealId, MintNftRequestDto request);
        Task<PagedResult<DealDto>> GetMyNftsAsync(int investorId, SieveModel sieveModel);
    }
}
