using AISEP.Data;
using AISEP.Repositories.Bookings;
using AISEP.Repositories.RefreshTokens;

namespace AISEP.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IBookingRepository? _bookings;
        private IRefreshTokenRepository? _refreshTokens;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

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
