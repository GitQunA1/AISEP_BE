using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using Sieve.Models;

namespace AISEP.BLL.Services.AdvisorBankAccounts
{
    public interface IAdvisorBankAccountService
    {
        Task<PagedResult<AdvisorBankAccountResponse>> GetAllAsync(SieveModel model);
        Task<AdvisorBankAccountResponse?> GetByIdAsync(int id);
        Task<AdvisorBankAccountResponse?> GetMyAsync();
        Task<AdvisorBankAccountResponse> CreateAsync(CreateAdvisorBankAccountRequest request);
        Task<AdvisorBankAccountResponse> UpdateAsync(int id, UpdateAdvisorBankAccountRequest request);
        Task<AdvisorBankAccountResponse> DeactivateAsync(int id);
    }
}
