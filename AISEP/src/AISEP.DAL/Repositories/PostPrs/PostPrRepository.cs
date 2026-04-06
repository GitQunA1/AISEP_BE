using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.PostPrs
{
    public class PostPrRepository : IPostPrRepository
    {
        private readonly ApplicationDbContext _context;

        public PostPrRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<PostPr> GetQuery()
        {
            return _context.PostPrs.AsQueryable();
        }

        public async Task<PostPr?> GetByIdAsync(int id)
        {
            return await _context.PostPrs.FirstOrDefaultAsync(p => p.PostPrId == id);
        }

        public async Task AddAsync(PostPr postPr)
        {
            await _context.PostPrs.AddAsync(postPr);
        }

        public void Update(PostPr postPr)
        {
            _context.PostPrs.Update(postPr);
        }

        public void Delete(PostPr postPr)
        {
            _context.PostPrs.Remove(postPr);
        }
    }
}
