using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.FormValidationRules
{
    public interface IFormValidationRuleRepository
    {
        Task<List<FormValidationRule>> GetByFormKeyAsync(string formKey);
        Task<FormValidationRule?> GetByFormAndFieldAsync(string formKey, string fieldKey);
        Task<FormValidationRule?> GetByIdAsync(int id);
        Task AddAsync(FormValidationRule rule);
        void Update(FormValidationRule rule);
    }
}
