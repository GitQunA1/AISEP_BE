using AISEP.BLL.DTOs.Responses;
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

        public async Task<CollectedBookingCommissionSummaryResponse> GetCollectedBookingCommissionSummaryAsync()
        {
            var paidBookingIds = _unitOfWork.Transactions.GetQuery()
                .Where(t => t.Status == TransactionStatus.Completed
                            && t.ReferenceType == ReferenceType.Booking.ToString()
                            && t.ReferenceId.HasValue)
                .Select(t => t.ReferenceId!.Value)
                .Distinct();

            var items = await _unitOfWork.Bookings.GetBookingQuery()
                .Where(b => paidBookingIds.Contains(b.BookingId)
                            && b.SystemCommissionAmount > 0m)
                .Select(b => new CollectedBookingCommissionItemResponse
                {
                    BookingId = b.BookingId,
                    CommissionPercent = b.SystemCommissionConfig != null ? b.SystemCommissionConfig.Percent : 0m,
                    CommissionAmount = b.SystemCommissionAmount
                })
                .OrderByDescending(x => x.BookingId)
                .ToListAsync();

            return new CollectedBookingCommissionSummaryResponse
            {
                TotalCommissionAmount = items.Sum(x => x.CommissionAmount),
                BookingCount = items.Count,
                Items = items
            };
        }
    }
}
