using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Transactions
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int transactionId, int userId);
        Task<Transaction?> GetPendingByUserAndReferenceAsync(int userId, string referenceType, int referenceId);
        Task<Transaction?> GetPendingByPaymentCodeAsync(string paymentCode);
        Task<Transaction?> GetByPaymentCodeAsync(string paymentCode);
        Task AddAsync(Transaction transaction);
        void Update(Transaction transaction);
    }
}
