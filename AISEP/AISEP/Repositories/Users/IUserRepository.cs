using AISEP.Models.Entities;

namespace AISEP.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByProjectId(int id);
    }
}
