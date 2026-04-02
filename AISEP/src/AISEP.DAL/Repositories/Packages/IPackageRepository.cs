using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.DAL.Repositories.Packages
{
    public interface IPackageRepository
    {
        Task<IEnumerable<Package>> GetAllAsync();
        Task<IEnumerable<Package>> GetByRoleAsync(UserRole role);
        Task<Package?> GetByIdAsync(int packageId);
    }
}
