using AISEP.Models.Entities;

namespace AISEP.Repositories.Documents
{
    public interface IDocumentRepository
    {
        Task<Document?> GetByIdAsync(int id);
        Task<IEnumerable<Document>> GetByStartupIdAsync(int startupId);
        Task<IEnumerable<Document>> GetAllAsync();
        Task AddAsync(Document document);
        void Update(Document document);
        void Delete(Document document);
        IQueryable<Document> GetQueryable();
    }
}
