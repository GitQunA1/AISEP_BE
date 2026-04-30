using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Reviews
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;
        public ReviewRepository(ApplicationDbContext context) { _context = context; }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public Task<Review?> GetByIdAsync(int id)
        {
            return _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(r => r.Reviewer)       
                .FirstOrDefaultAsync(r => r.ReviewId == id);
        }

        public async Task<decimal?> GetAverageRatingByAdvisorIdAsync(int advisorId)
        {
            var averageRating = await _context.Reviews
                .Where(r => r.Booking.AdvisorId == advisorId)
                .AverageAsync(r => (decimal?)r.Rating);

            return averageRating.HasValue
                ? Math.Round(averageRating.Value, 2, MidpointRounding.AwayFromZero)
                : null;
        }

        public IQueryable<Review> GetReviewQuery()
        {
            return _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Advisor)
                        .ThenInclude(a => a.User)
                .Include(r => r.Reviewer)
                .OrderBy(r => r.ReviewId)
                .AsNoTracking();
        }
    }
}
