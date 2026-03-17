using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Packages
{
    public class PackageRepository : IPackageRepository
    {
        private readonly ApplicationDbContext _context;

        public PackageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Package>> GetAllAsync()
            => await _context.Packages.OrderBy(p => p.Price).ToListAsync();

        public async Task<Package?> GetByIdAsync(int packageId)
            => await _context.Packages.FirstOrDefaultAsync(p => p.PackageId == packageId);
    }
}
