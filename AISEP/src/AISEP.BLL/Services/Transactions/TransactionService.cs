using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public TransactionService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdminTransactionResponse>> GetAllForAdminAsync(SieveModel model)
        {
            model ??= new SieveModel();

            var query = _unitOfWork.Transactions.GetQuery()
                .Include(t => t.User);

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                transaction => _mapper.Map<AdminTransactionResponse>(transaction));
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
