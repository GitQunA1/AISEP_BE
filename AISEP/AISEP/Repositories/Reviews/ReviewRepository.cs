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

        public Task<Review?> GetByIdAsync(Guid id)
        {
            return _context.Reviews
                .Include(r => r.Advisor)
                    .ThenInclude(a => a.User)   // ✅ Advisor → User → UserName
                .Include(r => r.Reviewer)       // ✅ Reviewer → UserName (trực tiếp, không cần ThenInclude)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
                _context.Reviews.Remove(review);
        }

        public IQueryable<Review> GetReviewQuery()
        {
            return _context.Reviews
                .Include(r => r.Advisor)
                    .ThenInclude(a => a.User)   // ✅ Thêm ThenInclude để lấy UserName
                .Include(r => r.Reviewer)       // ✅ Reviewer là User, có UserName trực tiếp
                .AsNoTracking();
        }
    }
}
