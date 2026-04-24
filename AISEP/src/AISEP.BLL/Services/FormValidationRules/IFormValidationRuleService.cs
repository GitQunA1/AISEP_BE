using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.FormValidationRules
{
    public interface IFormValidationRuleService
    {
        Task<FormValidationConfigResponse> GetByFormKeyAsync(string formKey);
        Task<FormValidationRuleResponse> CreateAsync(CreateFormValidationRuleRequest request);
        Task<FormValidationRuleResponse> UpdateAsync(int id, UpsertFormValidationRuleRequest request);
    }
}
