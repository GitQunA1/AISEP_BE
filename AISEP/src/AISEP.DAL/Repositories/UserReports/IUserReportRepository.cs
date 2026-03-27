using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.UserReports
{
    public interface IUserReportRepository
    {
        Task<UserReport?> GetByIdAsync(int id);
        Task AddAsync(UserReport report);
        void Update(UserReport report);
    }
}
