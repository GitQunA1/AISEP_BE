using AISEP.Repositories.Advisors;
using AISEP.Repositories.Bookings;
using AISEP.Repositories.Documents;
using AISEP.Repositories.Projects;
using AISEP.Repositories.RefreshTokens;
using AISEP.Repositories.Reviews;
using AISEP.Repositories.Startups;
using AISEP.Repositories.StartupFollowers;
using AISEP.Repositories.Investors;

namespace AISEP.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IBookingRepository Bookings { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IDocumentRepository Documents { get; }
        IReviewRepository Reviews { get; }
        IStartupFollowerRepository StartupFollowers { get; }
        IProjectRepository Projects { get; }
        IStartupRepository Startups { get; }
        IInvestorRepository Investors { get; }


        Task<int> SaveChangesAsync();
    }
}
