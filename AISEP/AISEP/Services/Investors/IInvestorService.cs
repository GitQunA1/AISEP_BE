using AISEP.Common;
using AISEP.DTOs.Requests;
using AISEP.DTOs.Responses;
using Sieve.Models;

namespace AISEP.Services.Investors
{
    public interface IInvestorService
    {
        Task<PagedResult<InvestorResponse>> GetAllAsync(SieveModel model);
        Task<InvestorResponse?> GetByIdAsync(int investorId);
        Task<InvestorResponse?> GetMyProfileAsync(int userId);
        Task<InvestorResponse?> CreateAsync(int userId, InvestorRequest dto);
        Task<InvestorResponse?> UpdateAsync(int userId, InvestorRequest dto);
    }
}
