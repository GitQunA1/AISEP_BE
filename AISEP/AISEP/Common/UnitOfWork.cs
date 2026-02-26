using AISEP.Data;
using AISEP.Repositories.Advisors;
using AISEP.Repositories.Bookings;
using AISEP.Repositories.Documents;
using AISEP.Repositories.RefreshTokens;
using AISEP.Repositories.Reviews;
using AISEP.Repositories.StartupFollowers;

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

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
        public IDocumentRepository Documents => _documents ??= new DocumentRepository(_context);
        //public IAdvisorsRepository Advisors => _advisors ??= new AdvisorRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
        public IStartupFollowerRepository StartupFollowers => _startupFollowers ??= new StartupFollowerRepository(_context);

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
