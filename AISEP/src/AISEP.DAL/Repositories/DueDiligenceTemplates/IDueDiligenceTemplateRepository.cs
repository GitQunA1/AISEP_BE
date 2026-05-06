using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.DueDiligenceTemplates
{
    public interface IDueDiligenceTemplateRepository
    {
        Task<DueDiligenceTemplate?> GetAsync();
        Task AddAsync(DueDiligenceTemplate template);
        void Update(DueDiligenceTemplate template);
    }
}
