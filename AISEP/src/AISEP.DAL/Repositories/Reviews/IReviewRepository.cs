using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Reviews
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task AddAsync(Review review);
        Task DeleteAsync(int id);
        IQueryable<Review> GetReviewQuery();
    }
}
