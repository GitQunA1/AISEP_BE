using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AISEP.DAL.Repositories.Transactions
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(int transactionId, int userId)
            => await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId);

        public async Task<Transaction?> GetPendingByUserAndReferenceAsync(int userId, string referenceType, int referenceId)
            => await _context.Transactions
                .FirstOrDefaultAsync(t => t.UserId == userId
                                       && t.ReferenceType == referenceType
                                       && t.ReferenceId == referenceId
                                       && t.Status == TransactionStatus.Pending);

        public async Task<Transaction?> GetPendingByPaymentCodeAsync(string paymentCode)
            => await _context.Transactions
                .FirstOrDefaultAsync(t => t.PaymentCode == paymentCode
                                       && t.Status == TransactionStatus.Pending);

        public async Task AddAsync(Transaction transaction)
            => await _context.Transactions.AddAsync(transaction);

        public void Update(Transaction transaction)
            => _context.Transactions.Update(transaction);
    }
}
