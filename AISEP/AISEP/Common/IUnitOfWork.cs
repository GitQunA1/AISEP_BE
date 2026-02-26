using AISEP.Repositories.Bookings;
using AISEP.Repositories.Documents;
using AISEP.Repositories.RefreshTokens;

namespace AISEP.Common
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IBookingRepository Bookings { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IDocumentRepository Documents { get; }

        // Save changes
        Task<int> SaveChangesAsync();
    }
}
