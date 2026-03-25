using AISEP.DAL.Repositories.Advisors;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.Chats;
using AISEP.DAL.Repositories.Documents;
using AISEP.DAL.Repositories.InvestorAIAnalyses;
using AISEP.DAL.Repositories.Packages;
using AISEP.DAL.Repositories.Projects;
using AISEP.DAL.Repositories.RefreshTokens;
using AISEP.DAL.Repositories.Reviews;
using AISEP.DAL.Repositories.Startups;
using AISEP.DAL.Repositories.StartupAIAnalyses;
using AISEP.DAL.Repositories.StartupFollowers;
using AISEP.DAL.Repositories.Investors;
using AISEP.DAL.Repositories.Subscriptions;
using AISEP.DAL.Repositories.Transactions;
using AISEP.DAL.Repositories.Users;
using AISEP.DAL.Repositories.WalletTransactions;
using AISEP.DAL.Repositories.UnlockedProjects;
using AISEP.DAL.Repositories.Notifications;

namespace AISEP.DAL.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IBookingRepository           Bookings           { get; }
        IRefreshTokenRepository      RefreshTokens      { get; }
        IDocumentRepository          Documents          { get; }
        IReviewRepository            Reviews            { get; }
        IStartupFollowerRepository   StartupFollowers   { get; }
        IProjectRepository           Projects           { get; }
        IStartupRepository           Startups           { get; }
        IInvestorRepository          Investors          { get; }
        IInvestorAIAnalysisRepository InvestorAIAnalyses { get; }
        IUserRepository              Users              { get; }
        IStartupAIAnalysisRepository StartupAIAnalyses  { get; }
        IAdvisorsRepository          Advisors           { get; }
        IChatSessionRepository       ChatSessions       { get; }
        IChatMessageRepository       ChatMessages       { get; }
        ITransactionRepository       Transactions       { get; }
        IPackageRepository           Packages           { get; }
        ISubscriptionRepository      Subscriptions      { get; }
        IUnlockedProjectRepository   UnlockedProjects   { get; }
        INotificationRepository      Notifications      { get; }
        IWalletTransactionRepository WalletTransactions { get; }

        Task<int> SaveChangesAsync();
    }
}
