using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.ScorecardWeightConfigs
{
    public interface IScorecardWeightConfigRepository
    {
        Task<ScorecardWeightConfig?> GetDefaultAsync();
        Task<ScorecardWeightConfig?> GetByIdAsync(int id);
        void Update(ScorecardWeightConfig config);
    }
}
