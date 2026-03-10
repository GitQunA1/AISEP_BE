using AISEP.DAL.Repositories.Advisors;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.Documents;
using AISEP.DAL.Repositories.Projects;
using AISEP.DAL.Repositories.RefreshTokens;
using AISEP.DAL.Repositories.Reviews;
using AISEP.DAL.Repositories.Startups;
using AISEP.DAL.Repositories.StartupAIAnalyses;
using AISEP.DAL.Repositories.StartupFollowers;
using AISEP.DAL.Repositories.Investors;
using AISEP.DAL.Repositories.Users;

namespace AISEP.DAL.Common
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
        IUserRepository Users { get; }
        IStartupAIAnalysisRepository StartupAIAnalyses { get; }


        Task<int> SaveChangesAsync();
    }
}
