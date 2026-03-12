using AISEP.BLL.Common;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Investors
{
    public interface IInvestorService
    {
        Task<PagedResult<InvestorResponse>> GetAllAsync(SieveModel model);
        Task<InvestorResponse?> GetByIdAsync(int investorId);
        Task<InvestorResponse?> GetMyProfileAsync(int userId);
        Task<InvestorResponse?> CreateAsync(int userId, CreateInvestorRequest dto);
        Task<InvestorResponse?> UpdateAsync(int userId, UpdateInvestorRequest dto);
    }
}
