using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Documents
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.DocumentId == id);
        }

        public async Task<IEnumerable<Document>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Documents
                .Where(d => d.ProjectId == projectId)
                .OrderByDescending(d => d.VerifiedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await _context.Documents
                .Include(d => d.Project)
                .OrderByDescending(d => d.VerifiedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
        }

        //public void Update(Document document)
        //{
        //    _context.Documents.Update(document);
        //}

        public void Delete(Document document)
        {
            _context.Documents.Remove(document);
        }

        public IQueryable<Document> GetQueryable()
        {
            return _context.Documents
                .Include(d => d.Project)
                .AsQueryable();
        }
    }
}
