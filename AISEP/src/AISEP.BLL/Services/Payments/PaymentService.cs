using System.Text.RegularExpressions;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Settings;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SePaySettings _sePaySettings;
        private readonly IMapper _mapper;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly INotificationService _notificationService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IOptions<SePaySettings> sePaySettings,
            IMapper mapper,
            ISieveProcessor sieveProcessor,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _sePaySettings = sePaySettings.Value;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<PackageResponse>> GetInvestorPackagesAsync()
        {
            return await GetPackagesByRoleAsync(UserRole.Investor);
        }

        public async Task<IEnumerable<PackageResponse>> GetStartupPackagesAsync()
        {
            return await GetPackagesByRoleAsync(UserRole.Startup);
        }

        public async Task<CheckoutResponse> CheckoutSubscriptionAsync(int userId, int packageId)
        {
            if (packageId <= 0)
                throw new InvalidOperationException("PackageId must be greater than 0.");

            var package = await _unitOfWork.Packages.GetByIdAsync(packageId)
                ?? throw new KeyNotFoundException("Package not found.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (package.TargetRole != user.Role)
                throw new InvalidOperationException("Selected package is not available for your role.");

            return await CreateOrReusePendingTransactionAsync(
                userId,
                ReferenceType.Subscription,
                packageId,
                package.Price);
        }

        public async Task<CheckoutResponse> CheckoutBookingAsync(int userId, int bookingId)
        {
            if (bookingId <= 0)
                throw new InvalidOperationException("BookingId must be greater than 0.");

            var booking = await _unitOfWork.Bookings.GetPayableByIdAndCustomerAsync(bookingId, userId);

            if (booking is null)
                throw new KeyNotFoundException("Booking not found or not in ApprovedAwaitingPayment status.");

            return await CreateOrReusePendingTransactionAsync(
                userId,
                ReferenceType.Booking,
                bookingId,
                booking.Price);
        }

        public async Task<PackageResponse> UpdatePackageAsync(int packageId, UpdatePackageRequest request)
        {
            if (packageId <= 0)
                throw new InvalidOperationException("PackageId must be greater than 0.");

            var package = await _unitOfWork.Packages.GetByIdAsync(packageId)
                ?? throw new KeyNotFoundException("Package not found.");

            if (package.TargetRole != UserRole.Investor && package.TargetRole != UserRole.Startup)
                throw new InvalidOperationException("Only Investor and Startup packages can be updated from this endpoint.");

            if (request.Price <= 0)
                throw new InvalidOperationException("Price must be greater than 0.");

            if (request.DurationMonths <= 0)
                throw new InvalidOperationException("DurationMonths must be greater than 0.");

            if (string.IsNullOrWhiteSpace(request.PackageName))
                throw new InvalidOperationException("PackageName is required.");

            package.PackageName = request.PackageName.Trim();
            package.Description = request.Description?.Trim();
            package.Price = request.Price;
            package.DurationMonths = request.DurationMonths;
            package.MaxAiRequests = request.MaxAiRequests;
            package.MaxProjectViews = request.MaxProjectViews;
            package.FreeBookingCount = request.FreeBookingCount;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PackageResponse>(package);
        }

        private async Task<CheckoutResponse> CreateOrReusePendingTransactionAsync(
            int userId,
            ReferenceType referenceType,
            int referenceId,
            decimal amount)
        {
            var referenceTypeText = referenceType.ToString();

            // Return existing pending transaction if one already exists.
            // If the pending transaction is too old, mark it as Failed and create a new one.
            var existingTransaction = await _unitOfWork.Transactions
                .GetPendingByUserAndReferenceAsync(userId, referenceTypeText, referenceId);

            if (existingTransaction is not null)
            {
                if (IsPendingExpired(existingTransaction))
                {
                    existingTransaction.Status = TransactionStatus.Failed;
                    _unitOfWork.Transactions.Update(existingTransaction);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    var existingResponse = _mapper.Map<CheckoutResponse>(existingTransaction);
                    existingResponse.QrCodeUrl = BuildQrCodeUrl(existingTransaction.Amount, existingTransaction.PaymentCode!);
                    return existingResponse;
                }
            }

            var transaction = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = TransactionType.Payment,
                Status = TransactionStatus.Pending,
                ReferenceType = referenceTypeText,
                ReferenceId = referenceId
            };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            transaction.PaymentCode = $"{_sePaySettings.PaymentPrefix}{transaction.TransactionId}";
            await _unitOfWork.SaveChangesAsync();

            var response = _mapper.Map<CheckoutResponse>(transaction);
            response.QrCodeUrl = BuildQrCodeUrl(transaction.Amount, transaction.PaymentCode!);
            return response;
        }

        public async Task<TransactionStatusResponse> GetTransactionStatusAsync(int userId, int transactionId)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId, userId);

            if (transaction is null)
                throw new KeyNotFoundException("Transaction not found.");

            // Auto-expire old pending transactions when clients poll status.
            if (transaction.Status == TransactionStatus.Pending && IsPendingExpired(transaction))
            {
                transaction.Status = TransactionStatus.Failed;
                _unitOfWork.Transactions.Update(transaction);
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<TransactionStatusResponse>(transaction);
        }

        public async Task<BookingPaymentStatusResponse> GetBookingPaymentStatusAsync(int userId, int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.CustomerId != userId)
                throw new InvalidOperationException("You do not have permission to view this booking payment status.");

            var latest = await _unitOfWork.Transactions.GetLatestByUserAndReferenceAsync(
                userId,
                ReferenceType.Booking.ToString(),
                bookingId);

            if (latest is not null && latest.Status == TransactionStatus.Pending && IsPendingExpired(latest))
            {
                latest.Status = TransactionStatus.Failed;
                _unitOfWork.Transactions.Update(latest);
                await _unitOfWork.SaveChangesAsync();
            }

            return new BookingPaymentStatusResponse
            {
                BookingId = booking.BookingId,
                BookingStatus = booking.Status.ToString(),
                IsPaid = booking.Status == BookingStatus.Confirmed || booking.Price == 0,
                TransactionId = latest?.TransactionId,
                TransactionStatus = latest?.Status.ToString(),
                PaymentCode = latest?.PaymentCode,
                Amount = booking.Price
            };
        }

        public async Task<PagedResult<BookingPaymentTransactionResponse>> GetBookingPaymentTransactionsAsync(int userId, SieveModel model)
        {
            var query = _unitOfWork.Transactions.GetByUserAndReferenceTypeQuery(userId, ReferenceType.Booking.ToString());

            return await PaginationHelper.PaginateAsync(
                query,
                model,
                _sieveProcessor,
                x => _mapper.Map<BookingPaymentTransactionResponse>(x));
        }

        public async Task ProcessSePayWebhookAsync(SePayWebhookRequest request)
        {
            var paymentCode = ExtractPaymentCode(request);

            if (string.IsNullOrEmpty(paymentCode))
                throw new KeyNotFoundException("Không tìm thấy PaymentCode trong payload webhook.");

            var transaction = await _unitOfWork.Transactions.GetPendingByPaymentCodeAsync(paymentCode);

            if (transaction is null)
            {
                var anyTransaction = await _unitOfWork.Transactions.GetByPaymentCodeAsync(paymentCode);

                // Idempotency: webhook can be sent more than once.
                if (anyTransaction is not null && anyTransaction.Status == TransactionStatus.Completed)
                {
                    return;
                }

                throw new KeyNotFoundException("Transaction không tồn tại hoặc không còn ở trạng thái Pending.");
            }

            if (request.TransferAmount < transaction.Amount)
                throw new InvalidOperationException("Transfer amount is less than required.");

            transaction.Status = TransactionStatus.Completed;
            transaction.SepayTransactionId = request.ReferenceCode;
            transaction.PaymentContent = request.Content
                                      ?? request.Description
                                      ?? request.Code;
            transaction.CompletedAt = DateTime.UtcNow;

            await ActivateServiceAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ActivateServiceAsync(Transaction transaction)
        {
            if (!Enum.TryParse<ReferenceType>(transaction.ReferenceType, out var referenceType))
                throw new InvalidOperationException($"Invalid ReferenceType: {transaction.ReferenceType}");

            if (transaction.ReferenceId is null)
                throw new InvalidOperationException("Transaction is missing ReferenceId.");

            switch (referenceType)
            {
                case ReferenceType.Subscription:
                    await ActivateSubscriptionAsync(transaction);
                    break;

                case ReferenceType.Booking:
                    await ConfirmBookingAsync(transaction);
                    break;
            }
        }

        private async Task ActivateSubscriptionAsync(Transaction transaction)
        {
            var package = await _unitOfWork.Packages.GetByIdAsync(transaction.ReferenceId!.Value);

            if (package is null)
                throw new InvalidOperationException($"Package {transaction.ReferenceId} not found.");

            // Prevent overlap: extension starts at max(current active end, now).
            var now = DateTime.UtcNow;
            var latestActive = await _unitOfWork.Subscriptions.GetLatestActiveAsync(transaction.UserId);
            var startDate = latestActive is not null && latestActive.EndDate > now
                ? latestActive.EndDate
                : now;
            var endDate = startDate.AddMonths(package.DurationMonths);

            await _unitOfWork.Subscriptions.AddAsync(new Subscription
            {
                PackageId = package.PackageId,
                UserId = transaction.UserId,
                StartDate = startDate,
                EndDate = endDate,
                Status = SubscriptionStatus.Active,
                RemainingFreeBookings = package.FreeBookingCount,
                UsedAiRequests = 0,
                UsedProjectViews = 0
            });

            var user = await _unitOfWork.Users.GetByIdAsync(transaction.UserId);
            if (user is not null)
            {
                user.IsPremium = true;
            }
        }

        private async Task ConfirmBookingAsync(Transaction transaction)
        {
            var booking = await _unitOfWork.Bookings
                .GetByIdWithAdvisorWalletAsync(transaction.ReferenceId!.Value);

            if (booking is null)
                throw new InvalidOperationException($"Booking {transaction.ReferenceId} not found.");

            if (booking.Status != BookingStatus.ApprovedAwaitingPayment)
                return;

            booking.Status = BookingStatus.Confirmed;
            await _notificationService.SendNotificationAsync(
                booking.CustomerId,
                "Booking payment successful",
                $"Payment for booking #{booking.BookingId} was successful.",
                NotificationType.General,
                booking.BookingId,
                "Booking");
            await _notificationService.SendNotificationAsync(
                booking.Advisor.UserId,
                "Booking confirmed",
                $"Booking #{booking.BookingId} has been confirmed after payment.",
                NotificationType.General,
                booking.BookingId,
                "Booking");
        }

        private async Task<IEnumerable<PackageResponse>> GetPackagesByRoleAsync(UserRole role)
        {
            var packages = await _unitOfWork.Packages.GetByRoleAsync(role);
            return _mapper.Map<IEnumerable<PackageResponse>>(packages);
        }

        private string BuildQrCodeUrl(decimal amount, string paymentCode)
        {
            var accountName = Uri.EscapeDataString(_sePaySettings.AccountName);
            return $"https://img.vietqr.io/image/{_sePaySettings.BankCode}-{_sePaySettings.AccountNumber}-compact2.jpg?amount={amount:0}&addInfo={paymentCode}&accountName={accountName}";
        }

        private bool IsPendingExpired(Transaction transaction)
        {
            var timeoutMinutes = _sePaySettings.PendingTimeoutMinutes > 0
                ? _sePaySettings.PendingTimeoutMinutes
                : 30;

            return transaction.CreatedAt <= DateTime.UtcNow.AddMinutes(-timeoutMinutes);
        }

        private string? ExtractPaymentCode(SePayWebhookRequest request)
        {
            var candidates = new[]
            {
                request.Content,
                request.Description,
                request.Code,
                request.ReferenceCode
            };

            foreach (var candidate in candidates)
            {
                var extracted = ExtractPaymentCodeFromText(candidate);
                if (!string.IsNullOrWhiteSpace(extracted))
                {
                    return extracted;
                }
            }

            return null;
        }

        private string? ExtractPaymentCodeFromText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var pattern = $@"(?i){Regex.Escape(_sePaySettings.PaymentPrefix)}\s*(\d+)";
            var match = Regex.Match(text, pattern);

            if (!match.Success)
                return null;

            return $"{_sePaySettings.PaymentPrefix}{match.Groups[1].Value}";
        }
    }
}
