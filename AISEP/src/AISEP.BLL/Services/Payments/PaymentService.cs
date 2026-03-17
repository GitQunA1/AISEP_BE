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
            else // Booking
            {
                var booking = await _unitOfWork.Bookings.GetPendingByIdAndCustomerAsync(request.ReferenceId, userId);

                if (booking is null)
                    throw new KeyNotFoundException("Booking not found or not in Pending status.");

                amount = booking.Price;
            }

            // Return existing pending transaction if one already exists
            var existingTransaction = await _unitOfWork.Transactions
                .GetPendingByUserAndReferenceAsync(userId, referenceType, request.ReferenceId);

            if (existingTransaction is not null)
            {
                return new CheckoutResponse
                {
                    TransactionId = existingTransaction.TransactionId,
                    Amount        = existingTransaction.Amount,
                    PaymentCode   = existingTransaction.PaymentCode!,
                    QrCodeUrl     = BuildQrCodeUrl(existingTransaction.Amount, existingTransaction.PaymentCode!)
                };
            }

            // Create new transaction
            var transaction = new Transaction
            {
                UserId        = userId,
                Amount        = amount,
                Type          = TransactionType.Payment,
                Status        = TransactionStatus.Pending,
                ReferenceType = referenceType,
                ReferenceId   = request.ReferenceId
            };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Generate PaymentCode = Prefix + TransactionId (requires the ID from DB)
            transaction.PaymentCode = $"{_sePaySettings.PaymentPrefix}{transaction.TransactionId}";
            await _unitOfWork.SaveChangesAsync();

            return new CheckoutResponse
            {
                TransactionId = transaction.TransactionId,
                Amount        = transaction.Amount,
                PaymentCode   = transaction.PaymentCode,
                QrCodeUrl     = BuildQrCodeUrl(transaction.Amount, transaction.PaymentCode)
            };
        }

        public async Task<TransactionStatusResponse> GetTransactionStatusAsync(int userId, int transactionId)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId, userId);

            if (transaction is null)
                throw new KeyNotFoundException("Transaction not found.");

            return new TransactionStatusResponse
            {
                TransactionId = transaction.TransactionId,
                Status        = transaction.Status.ToString(),
                PaymentCode   = transaction.PaymentCode ?? string.Empty,
                Amount        = transaction.Amount
            };
        }

        public async Task ProcessSePayWebhookAsync(SePayWebhookRequest request)
        {
            var paymentCode = ExtractPaymentCode(request.Content);

            if (string.IsNullOrEmpty(paymentCode))
                throw new KeyNotFoundException("Transaction not found or already processed.");

            var transaction = await _unitOfWork.Transactions.GetPendingByPaymentCodeAsync(paymentCode);

            if (transaction is null)
                throw new KeyNotFoundException("Transaction not found or already processed.");

            if (request.TransferAmount < transaction.Amount)
                throw new InvalidOperationException("Transfer amount is less than required.");

            // Mark transaction as Completed
            transaction.Status             = TransactionStatus.Completed;
            transaction.SepayTransactionId = request.ReferenceCode;
            transaction.PaymentContent     = request.Content;
            transaction.CompletedAt        = DateTime.UtcNow;

            // Activate the purchased service
            await ActivateServiceAsync(transaction);

            await _unitOfWork.SaveChangesAsync();
        }

        // ── Post-Payment Activation ──────────────────────────────────────

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

            await _unitOfWork.Subscriptions.AddAsync(new Subscription
            {
                PackageId = package.PackageId,
                UserId    = transaction.UserId,
                StartDate = DateTime.UtcNow,
                EndDate   = DateTime.UtcNow.AddMonths(package.DurationMonths),
                Status    = SubscriptionStatus.Active
            });

            // Set User as Premium
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

            // Only confirm if booking is still Pending
            if (booking.Status != BookingStatus.Pending)
                return;

            // Update booking status to Confirmed
            booking.Status = BookingStatus.Confirmed;

            // Credit Advisor's wallet
            if (booking.Advisor?.Wallet is not null)
            {
                booking.Advisor.Wallet.Balance += transaction.Amount;

                await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                {
                    WalletId  = booking.Advisor.Wallet.WalletId,
                    Amount    = transaction.Amount,
                    Type      = WalletTransactionType.Deposit,
                    Status    = WalletTransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private string BuildQrCodeUrl(decimal amount, string paymentCode)
        {
            var accountName = Uri.EscapeDataString(_sePaySettings.AccountName);
            return $"https://img.vietqr.io/image/{_sePaySettings.BankCode}-{_sePaySettings.AccountNumber}-compact2.jpg?amount={amount:0}&addInfo={paymentCode}&accountName={accountName}";
        }

        private string? ExtractPaymentCode(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            var pattern = $@"(?i){Regex.Escape(_sePaySettings.PaymentPrefix)}\s*(\d+)";
            var match   = Regex.Match(content, pattern);

            if (!match.Success)
                return null;

            return $"{_sePaySettings.PaymentPrefix}{match.Groups[1].Value}";
        }
    }
}
