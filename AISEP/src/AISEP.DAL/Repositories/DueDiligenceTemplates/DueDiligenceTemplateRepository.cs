using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.DueDiligenceTemplates
{
    public class DueDiligenceTemplateRepository : IDueDiligenceTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public DueDiligenceTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DueDiligenceTemplate?> GetAsync()
            => await _context.DueDiligenceTemplates
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

        public async Task AddAsync(DueDiligenceTemplate template)
            => await _context.DueDiligenceTemplates.AddAsync(template);

        public void Update(DueDiligenceTemplate template)
            => _context.DueDiligenceTemplates.Update(template);
    }
}
