using AISEP.DTOs;
using Sieve.Models;

namespace AISEP.Services.Investors
{
    public interface IInvestorService
    {
        Task<PagedResultDto<InvestorResponseDto>> GetAllAsync(SieveModel model);
        Task<InvestorResponseDto?> GetByIdAsync(int investorId);
        Task<InvestorResponseDto?> GetMyProfileAsync(int userId);
        Task<InvestorResponseDto?> CreateAsync(int userId, InvestorDto dto);
        Task<InvestorResponseDto?> UpdateAsync(int userId, InvestorDto dto);
    }
}
