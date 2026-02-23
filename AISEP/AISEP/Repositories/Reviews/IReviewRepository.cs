using AISEP.Models.Entities;

namespace AISEP.Repositories.Reviews
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task AddAsync(Review review);
        Task DeleteAsync(Guid id);
        IQueryable<Review> GetReviewQuery();
    }
}
