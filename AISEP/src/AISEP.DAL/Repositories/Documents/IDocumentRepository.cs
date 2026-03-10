using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Documents
{
    public interface IDocumentRepository
    {
        Task<Document?> GetByIdAsync(int id);
        Task<IEnumerable<Document>> GetByProjectIdAsync(int projcetId);
        Task<IEnumerable<Document>> GetAllAsync();
        Task AddAsync(Document document);

        //void Update(Document document);
        void Delete(Document document);
        IQueryable<Document> GetQueryable();
    }
}
