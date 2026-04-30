using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Reviews
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task AddAsync(Review review);
        Task<decimal?> GetAverageRatingByAdvisorIdAsync(int advisorId);
        IQueryable<Review> GetReviewQuery();
    }
}
