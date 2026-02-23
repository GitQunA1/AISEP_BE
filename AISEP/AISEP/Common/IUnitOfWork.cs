using AISEP.Repositories.Advisors;
using AISEP.Repositories.Bookings;
using AISEP.Repositories.RefreshTokens;
using AISEP.Repositories.Reviews;

namespace AISEP.Common
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IBookingRepository Bookings { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        //IAdvisorsRepository Advisors { get; }

        IReviewRepository Reviews { get; }
        // Add more repositories here as needed

        // Save changes
        Task<int> SaveChangesAsync();
    }
}
