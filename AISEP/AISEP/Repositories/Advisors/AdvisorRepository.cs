using AISEP.Data;

namespace AISEP.Repositories.Advisors
{
    public class AdvisorRepository
    {
        private readonly ApplicationDbContext _context;

        public AdvisorRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
