using AISEP.DAL.Repositories.Advisors;
using AISEP.DAL.Repositories.AdvisorAvailabilities;
using AISEP.DAL.Repositories.AdvisorBankAccounts;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.BookingSlots;
using AISEP.DAL.Repositories.Chats;
using AISEP.DAL.Repositories.ConsultingReports;
using AISEP.DAL.Repositories.Documents;
using AISEP.DAL.Repositories.InvestorAIAnalyses;
using AISEP.DAL.Repositories.Packages;
using AISEP.DAL.Repositories.PremiumFreeBookingUsageLogs;
using AISEP.DAL.Repositories.Projects;
using AISEP.DAL.Repositories.ProjectAdvisorAssignments;
using AISEP.DAL.Repositories.RefreshTokens;
using AISEP.DAL.Repositories.Reviews;
using AISEP.DAL.Repositories.Startups;
using AISEP.DAL.Repositories.StartupAIAnalyses;
using AISEP.DAL.Repositories.ProjectFollowers;
using AISEP.DAL.Repositories.Investors;
using AISEP.DAL.Repositories.Payouts;
using AISEP.DAL.Repositories.PayoutGroups;
using AISEP.DAL.Repositories.Subscriptions;
using AISEP.DAL.Repositories.Transactions;
using AISEP.DAL.Repositories.Users;
using AISEP.DAL.Repositories.UserReports;
using AISEP.DAL.Repositories.WalletTransactions;
using AISEP.DAL.Repositories.Wallets;
using AISEP.DAL.Repositories.UnlockedProjects;
using AISEP.DAL.Repositories.Notifications;
using AISEP.DAL.Repositories.ConnectionRequests;
using AISEP.DAL.Repositories.Deals;
using AISEP.DAL.Repositories.PostPrs;
using AISEP.DAL.Repositories.FormValidationRules;
using AISEP.DAL.Repositories.SystemCommissionConfigs;
namespace AISEP.DAL.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IBookingRepository           Bookings           { get; }
        IBookingSlotRepository       BookingSlots       { get; }
        IRefreshTokenRepository      RefreshTokens      { get; }
        IDocumentRepository          Documents          { get; }
        IReviewRepository            Reviews            { get; }
        IProjectFollowerRepository   ProjectFollowers   { get; }
        IProjectRepository           Projects           { get; }
        IProjectAdvisorAssignmentRepository ProjectAdvisorAssignments { get; }
        IStartupRepository           Startups           { get; }
        IInvestorRepository          Investors          { get; }
        IInvestorAIAnalysisRepository InvestorAIAnalyses { get; }
        IUserRepository              Users              { get; }
        IUserReportRepository        UserReports        { get; }
        IStartupAIAnalysisRepository StartupAIAnalyses  { get; }
        IAdvisorsRepository          Advisors           { get; }
        IAdvisorAvailabilityRepository AdvisorAvailabilities { get; }
        IAdvisorBankAccountRepository AdvisorBankAccounts { get; }
        IChatSessionRepository       ChatSessions       { get; }
        IChatMessageRepository       ChatMessages       { get; }
        IConsultingReportRepository  ConsultingReports  { get; }
        ITransactionRepository       Transactions       { get; }
        IPackageRepository           Packages           { get; }
        ISubscriptionRepository      Subscriptions      { get; }
        IPremiumFreeBookingUsageLogRepository PremiumFreeBookingUsageLogs { get; }
        IUnlockedProjectRepository   UnlockedProjects   { get; }
        INotificationRepository      Notifications      { get; }
        IWalletTransactionRepository WalletTransactions { get; }
        IWalletRepository            Wallets            { get; }
        IPayoutRepository     Payouts     { get; }
        IPayoutGroupRepository PayoutGroups { get; }
        ISystemCommissionConfigRepository SystemCommissionConfigs { get; }
        IConnectionRequestRepository ConnectionRequests { get; }
        IDealRepository              Deals              { get; }
        IPostPrRepository            PostPrs            { get; }
        IFormValidationRuleRepository FormValidationRules { get; }

        Task<int> SaveChangesAsync();
    }
}


