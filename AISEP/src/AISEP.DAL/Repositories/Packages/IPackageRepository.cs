using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Packages
{
    public interface IPackageRepository
    {
        Task<IEnumerable<Package>> GetAllAsync();
        Task<Package?> GetByIdAsync(int packageId);
    }
}
