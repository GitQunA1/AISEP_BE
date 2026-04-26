using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.FormValidationRules
{
    public interface IFormValidationRuleService
    {
        Task<PagedResult<FormValidationRuleResponse>> GetByFormKeyAsync(string formKey, SieveModel model);
        Task<FormValidationRuleResponse> CreateAsync(CreateFormValidationRuleRequest request);
        Task<FormValidationRuleResponse> UpdateAsync(int id, UpsertFormValidationRuleRequest request);
    }
}
