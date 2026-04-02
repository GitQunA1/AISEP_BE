using AISEP.DAL.Entities;

namespace AISEP.DAL.Repositories.Transactions
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int transactionId, int userId);
        Task<Transaction?> GetLatestByUserAndReferenceAsync(int userId, string referenceType, int referenceId);
        Task<Transaction?> GetPendingByUserAndReferenceAsync(int userId, string referenceType, int referenceId);
        IQueryable<Transaction> GetByUserAndReferenceTypeQuery(int userId, string referenceType);
        Task<Transaction?> GetPendingByPaymentCodeAsync(string paymentCode);
        Task<Transaction?> GetByPaymentCodeAsync(string paymentCode);
        Task AddAsync(Transaction transaction);
        void Update(Transaction transaction);
    }
}
