namespace AISEP.BLL.Services.Transactions
{
    public interface ITransactionService
    {
        Task<List<int>> GetCollectedBookingCommissionIdsAsync();
    }
}
