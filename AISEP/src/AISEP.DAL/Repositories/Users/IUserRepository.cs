using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByProjectId(int id);
    }
}
