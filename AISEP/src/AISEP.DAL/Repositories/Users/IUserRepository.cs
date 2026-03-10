using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByProjectId(int id);
    }
}
