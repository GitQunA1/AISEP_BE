using AISEP.Data;
using AISEP.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.Repositories.Reviews
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
                .Include(r => r.Advisor)
                    .ThenInclude(a => a.User)   
                .Include(r => r.Reviewer)       
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
                _context.Reviews.Remove(review);
        }

        public IQueryable<Review> GetReviewQuery()
        {
            return _context.Reviews
                .Include(r => r.Advisor)
                    .ThenInclude(a => a.User)  
                .Include(r => r.Reviewer)       
                .AsNoTracking();
        }
    }
}
