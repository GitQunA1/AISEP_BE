using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.PostPrs
{
    public interface IPostPrRepository
    {
        IQueryable<PostPr> GetQuery();
        Task<PostPr?> GetByIdAsync(int id);
        Task AddAsync(PostPr postPr);
        void Update(PostPr postPr);
    }
}
