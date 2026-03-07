using AISEP.Data;
using AISEP.Repositories.Advisors;
using AISEP.Repositories.Bookings;
using AISEP.Repositories.Documents;
using AISEP.Repositories.Projects;
using AISEP.Repositories.RefreshTokens;
using AISEP.Repositories.Reviews;
using AISEP.Repositories.Startups;
using AISEP.Repositories.StartupFollowers;
using AISEP.Repositories.Investors;
using AISEP.Repositories.Users;

namespace AISEP.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IBookingRepository? _bookings;
        private IRefreshTokenRepository? _refreshTokens;
        private IAdvisorsRepository? _advisors;
        private IReviewRepository? _reviews;
        private IStartupFollowerRepository? _startupFollowers;
        private IDocumentRepository? _documents;
        private IProjectRepository? _projects;
        private IStartupRepository? _startups;
        private IInvestorRepository? _investors;
        private IUserRepository? _users;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBookingRepository Bookings         => _bookings         ??= new BookingRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokens  ??= new RefreshTokenRepository(_context);
        public IDocumentRepository Documents        => _documents        ??= new DocumentRepository(_context);
        public IReviewRepository Reviews            => _reviews          ??= new ReviewRepository(_context);
        public IStartupFollowerRepository StartupFollowers => _startupFollowers ??= new StartupFollowerRepository(_context);
        public IProjectRepository Projects          => _projects         ??= new ProjectRepository(_context);
        public IStartupRepository Startups          => _startups         ??= new StartupRepository(_context);
        public IInvestorRepository Investors        => _investors        ??= new InvestorRepository(_context);
        public IUserRepository Users                    => _users            ??= new UserRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
