using AISEP.DAL.Common;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.BLL.Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<int>> GetCollectedBookingCommissionIdsAsync()
        {
            var paidBookingIds = _unitOfWork.Transactions.GetQuery()
                .Where(t => t.Status == TransactionStatus.Completed
                            && t.ReferenceType == ReferenceType.Booking.ToString()
                            && t.ReferenceId.HasValue)
                .Select(t => t.ReferenceId!.Value)
                .Distinct();

            return await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => paidBookingIds.Contains(b.BookingId)
                            && b.SystemCommissionAmount > 0m)
                .Select(b => b.BookingId)
                .Distinct()
                .OrderByDescending(id => id)
                .ToListAsync();
        }
    }
}
