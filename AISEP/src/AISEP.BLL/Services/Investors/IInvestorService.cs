using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.Investors
{
    public interface IInvestorService
    {
        Task<PagedResult<InvestorResponse>> GetAllAsync(SieveModel model);
        Task<PagedResult<InvestorResponse>> GetMatchingInvestorsForCurrentStartupAsync(SieveModel model);
        Task<InvestorResponse?> GetByIdAsync(int investorId);
        Task<InvestorResponse?> GetMyProfileAsync();
        Task<InvestorResponse?> CreateAsync(CreateInvestorRequest dto);
        Task<InvestorResponse?> UpdateAsync(int id, UpdateInvestorRequest dto);
        Task ApproveInvestorAsync(int investorId);
        Task RejectInvestorAsync(int investorId, string rejectionReason);
    }
}
