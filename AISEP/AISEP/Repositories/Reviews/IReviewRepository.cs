using AISEP.Models.Entities;

namespace AISEP.Repositories.Reviews
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task AddAsync(Review review);
        Task DeleteAsync(int id);
        IQueryable<Review> GetReviewQuery();
    }
}
