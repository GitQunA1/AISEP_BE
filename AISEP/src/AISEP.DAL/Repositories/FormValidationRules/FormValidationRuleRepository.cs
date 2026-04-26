using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.FormValidationRules
{
    public class FormValidationRuleRepository : IFormValidationRuleRepository
    {
        private readonly ApplicationDbContext _context;

        public FormValidationRuleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<FormValidationRule> GetAllQuery()
            => _context.FormValidationRules.OrderBy(x => x.FieldKey).AsQueryable();

        public async Task<List<FormValidationRule>> GetByFormKeyAsync(string formKey)
            => await _context.FormValidationRules
                .Where(x => x.FormKey == formKey)
                .OrderBy(x => x.FieldKey)
                .AsNoTracking()
                .ToListAsync();

        public async Task<FormValidationRule?> GetByFormAndFieldAsync(string formKey, string fieldKey)
            => await _context.FormValidationRules
                .FirstOrDefaultAsync(x => x.FormKey == formKey && x.FieldKey == fieldKey);

        public async Task<FormValidationRule?> GetByIdAsync(int id)
            => await _context.FormValidationRules
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task AddAsync(FormValidationRule rule)
            => await _context.FormValidationRules.AddAsync(rule);

        public void Update(FormValidationRule rule)
            => _context.FormValidationRules.Update(rule);
    }
}
