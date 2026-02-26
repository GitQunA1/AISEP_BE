using AISEP.Repositories.Advisors;
using AISEP.Repositories.Bookings;
using AISEP.Repositories.Documents;
using AISEP.Repositories.RefreshTokens;
using AISEP.Repositories.Reviews;
using AISEP.Repositories.StartupFollowers;

namespace AISEP.Common
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IBookingRepository Bookings { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IDocumentRepository Documents { get; }

        //IAdvisorsRepository Advisors { get; }

        IReviewRepository Reviews { get; }
        IStartupFollowerRepository StartupFollowers { get; }

        // Add more repositories here as needed

        // Save changes
        Task<int> SaveChangesAsync();
    }
}
