using System.Text.RegularExpressions;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.Settings;
using AISEP.DAL.Data;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AISEP.BLL.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly SePaySettings _sePaySettings;

        public PaymentService(ApplicationDbContext dbContext, IOptions<SePaySettings> sePaySettings)
        {
            _dbContext = dbContext;
            _sePaySettings = sePaySettings.Value;
        }

        public async Task ProcessSePayWebhookAsync(SePayWebhookRequest request)
        {
            var paymentCode = ExtractPaymentCode(request.Content);

            if (string.IsNullOrEmpty(paymentCode))
                throw new KeyNotFoundException("Transaction not found or already processed.");

            var transaction = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.PaymentCode == paymentCode && t.Status == TransactionStatus.Pending);

            if (transaction is null)
                throw new KeyNotFoundException("Transaction not found or already processed.");

            if (request.TransferAmount < transaction.Amount)
                throw new InvalidOperationException("Transfer amount is less than required.");

            transaction.Status = TransactionStatus.Completed;
            transaction.SepayTransactionId = request.ReferenceCode;
            transaction.PaymentContent = request.Content;
            transaction.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private string? ExtractPaymentCode(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            var pattern = $@"(?i){Regex.Escape(_sePaySettings.PaymentPrefix)}\s*(\d+)";
            var match = Regex.Match(content, pattern);

            if (!match.Success)
                return null;

            return $"{_sePaySettings.PaymentPrefix}{match.Groups[1].Value}";
        }
    }
}
