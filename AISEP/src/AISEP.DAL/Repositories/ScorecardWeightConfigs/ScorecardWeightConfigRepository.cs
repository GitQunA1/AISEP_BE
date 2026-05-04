using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.ScorecardWeightConfigs
{
    public class ScorecardWeightConfigRepository : IScorecardWeightConfigRepository
    {
        private readonly ApplicationDbContext _context;

        public ScorecardWeightConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ScorecardWeightConfig?> GetDefaultAsync()
            => await _context.ScorecardWeightConfigs
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
    }
}
