using System.Text.RegularExpressions;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Settings;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.Extensions.Options;

namespace AISEP.BLL.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SePaySettings _sePaySettings;

        public PaymentService(IUnitOfWork unitOfWork, IOptions<SePaySettings> sePaySettings)
        {
            _unitOfWork = unitOfWork;
            _sePaySettings = sePaySettings.Value;
        }

        public async Task<IEnumerable<PackageResponse>> GetPackagesAsync()
        {
            var packages = await _unitOfWork.Packages.GetAllAsync();
            return packages.Select(p => new PackageResponse
            {
                PackageId      = p.PackageId,
                PackageName    = p.PackageName,
                Description    = p.Description,
                Price          = p.Price,
                DurationMonths = p.DurationMonths
            });
        }

        public async Task<CheckoutResponse> CheckoutAsync(int userId, CheckoutRequest request)
        {
            var referenceType = request.ReferenceType.ToString();

            decimal amount;

            if (request.ReferenceType == ReferenceType.Subscription)
            {
                var package = await _unitOfWork.Packages.GetByIdAsync(request.ReferenceId);

                if (package is null)
                    throw new KeyNotFoundException("Package not found.");

                amount = package.Price;
            }
            else
            {
                var booking = await _unitOfWork.Bookings.GetPendingByIdAndCustomerAsync(request.ReferenceId, userId);

                if (booking is null)
                    throw new KeyNotFoundException("Booking not found or not in Pending status.");

                amount = booking.Price;
            }

            // Return existing pending transaction if one already exists.
            // If the pending transaction is too old, mark it as Failed and create a new one.
            var existingTransaction = await _unitOfWork.Transactions
                .GetPendingByUserAndReferenceAsync(userId, referenceType, request.ReferenceId);

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
                    return new CheckoutResponse
                    {
                        TransactionId = existingTransaction.TransactionId,
                        Amount = existingTransaction.Amount,
                        PaymentCode = existingTransaction.PaymentCode!,
                        QrCodeUrl = BuildQrCodeUrl(existingTransaction.Amount, existingTransaction.PaymentCode!)
                    };
                }
            }

            var transaction = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = TransactionType.Payment,
                Status = TransactionStatus.Pending,
                ReferenceType = referenceType,
                ReferenceId = request.ReferenceId
            };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            transaction.PaymentCode = $"{_sePaySettings.PaymentPrefix}{transaction.TransactionId}";
            await _unitOfWork.SaveChangesAsync();

            return new CheckoutResponse
            {
                TransactionId = transaction.TransactionId,
                Amount = transaction.Amount,
                PaymentCode = transaction.PaymentCode,
                QrCodeUrl = BuildQrCodeUrl(transaction.Amount, transaction.PaymentCode)
            };
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

            return new TransactionStatusResponse
            {
                TransactionId = transaction.TransactionId,
                Status = transaction.Status.ToString(),
                PaymentCode = transaction.PaymentCode ?? string.Empty,
                Amount = transaction.Amount
            };
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
                Status = SubscriptionStatus.Active
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

            if (booking.Status != BookingStatus.Pending)
                return;

            booking.Status = BookingStatus.Confirmed;

            if (booking.Advisor?.Wallet is not null)
            {
                booking.Advisor.Wallet.Balance += transaction.Amount;

                await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                {
                    WalletId = booking.Advisor.Wallet.WalletId,
                    Amount = transaction.Amount,
                    Type = WalletTransactionType.Deposit,
                    Status = WalletTransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            }
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
