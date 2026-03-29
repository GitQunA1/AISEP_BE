using AISEP.DAL.Data;
using AISEP.DAL.Repositories.Advisors;
using AISEP.DAL.Repositories.AdvisorAvailabilities;
using AISEP.DAL.Repositories.Bookings;
using AISEP.DAL.Repositories.BookingSlots;
using AISEP.DAL.Repositories.Chats;
using AISEP.DAL.Repositories.ConsultingReports;
using AISEP.DAL.Repositories.Documents;
using AISEP.DAL.Repositories.InvestorAIAnalyses;
using AISEP.DAL.Repositories.Packages;
using AISEP.DAL.Repositories.Projects;
using AISEP.DAL.Repositories.ProjectAdvisorAssignments;
using AISEP.DAL.Repositories.RefreshTokens;
using AISEP.DAL.Repositories.Reviews;
using AISEP.DAL.Repositories.Startups;
using AISEP.DAL.Repositories.StartupAIAnalyses;
using AISEP.DAL.Repositories.ProjectFollowers;
using AISEP.DAL.Repositories.Investors;
using AISEP.DAL.Repositories.Subscriptions;
using AISEP.DAL.Repositories.Transactions;
using AISEP.DAL.Repositories.Users;
using AISEP.DAL.Repositories.UserReports;
using AISEP.DAL.Repositories.WalletTransactions;
using AISEP.DAL.Repositories.UnlockedProjects;
using AISEP.DAL.Repositories.Notifications;
using AISEP.DAL.Repositories.ConnectionRequests;
using AISEP.DAL.Repositories.Deals;
using AISEP.DAL.Repositories.NFTRecords;

namespace AISEP.DAL.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        private IBookingRepository?           _bookings;
        private IBookingSlotRepository?       _bookingSlots;
        private IRefreshTokenRepository?      _refreshTokens;
        private IAdvisorsRepository?          _advisors;
        private IAdvisorAvailabilityRepository? _advisorAvailabilities;
        private IChatSessionRepository?       _chatSessions;
        private IChatMessageRepository?       _chatMessages;
        private IConsultingReportRepository?  _consultingReports;
        private IReviewRepository?            _reviews;
        private IProjectFollowerRepository?   _projectFollowers;
        private IDocumentRepository?          _documents;
        private IProjectRepository?           _projects;
        private IProjectAdvisorAssignmentRepository? _projectAdvisorAssignments;
        private IStartupRepository?           _startups;
        private IInvestorRepository?          _investors;
        private IInvestorAIAnalysisRepository? _investorAIAnalyses;
        private IUserRepository?              _users;
        private IUserReportRepository?        _userReports;
        private IStartupAIAnalysisRepository? _startupAIAnalyses;
        private ITransactionRepository?       _transactions;
        private IPackageRepository?           _packages;
        private ISubscriptionRepository?      _subscriptions;
        private IUnlockedProjectRepository?   _unlockedProjects;
        private INotificationRepository?      _notifications;
        private IWalletTransactionRepository? _walletTransactions;
        private IConnectionRequestRepository? _connectionRequests;
        private IDealRepository?              _deals;
        private INFTRecordRepository?         _nftRecords;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IBookingRepository           Bookings           => _bookings           ??= new BookingRepository(_context);
        public IBookingSlotRepository       BookingSlots       => _bookingSlots       ??= new BookingSlotRepository(_context);
        public IRefreshTokenRepository      RefreshTokens      => _refreshTokens      ??= new RefreshTokenRepository(_context);
        public IDocumentRepository          Documents          => _documents          ??= new DocumentRepository(_context);
        public IReviewRepository            Reviews            => _reviews            ??= new ReviewRepository(_context);
        public IProjectFollowerRepository   ProjectFollowers   => _projectFollowers   ??= new ProjectFollowerRepository(_context);
        public IProjectRepository           Projects           => _projects           ??= new ProjectRepository(_context);
        public IProjectAdvisorAssignmentRepository ProjectAdvisorAssignments => _projectAdvisorAssignments ??= new ProjectAdvisorAssignmentRepository(_context);
        public IStartupRepository           Startups           => _startups           ??= new StartupRepository(_context);
        public IInvestorRepository          Investors          => _investors          ??= new InvestorRepository(_context);
        public IInvestorAIAnalysisRepository InvestorAIAnalyses => _investorAIAnalyses ??= new InvestorAIAnalysisRepository(_context);
        public IUserRepository              Users              => _users              ??= new UserRepository(_context);
        public IUserReportRepository        UserReports        => _userReports        ??= new UserReportRepository(_context);
        public IStartupAIAnalysisRepository StartupAIAnalyses  => _startupAIAnalyses ??= new StartupAIAnalysisRepository(_context);
        public IAdvisorsRepository          Advisors           => _advisors           ??= new AdvisorRepository(_context);
        public IAdvisorAvailabilityRepository AdvisorAvailabilities => _advisorAvailabilities ??= new AdvisorAvailabilityRepository(_context);
        public IChatSessionRepository       ChatSessions       => _chatSessions       ??= new ChatSessionRepository(_context);
        public IChatMessageRepository       ChatMessages       => _chatMessages       ??= new ChatMessageRepository(_context);
        public IConsultingReportRepository  ConsultingReports  => _consultingReports  ??= new ConsultingReportRepository(_context);
        public ITransactionRepository       Transactions       => _transactions       ??= new TransactionRepository(_context);
        public IPackageRepository           Packages           => _packages           ??= new PackageRepository(_context);
        public ISubscriptionRepository      Subscriptions      => _subscriptions      ??= new SubscriptionRepository(_context);
        public IUnlockedProjectRepository   UnlockedProjects   => _unlockedProjects   ??= new UnlockedProjectRepository(_context);
        public INotificationRepository      Notifications      => _notifications      ??= new NotificationRepository(_context);
        public IWalletTransactionRepository WalletTransactions => _walletTransactions ??= new WalletTransactionRepository(_context);
        public IConnectionRequestRepository ConnectionRequests => _connectionRequests ??= new ConnectionRequestRepository(_context);
        public IDealRepository              Deals              => _deals              ??= new DealRepository(_context);
        public INFTRecordRepository         NFTRecords         => _nftRecords         ??= new NFTRecordRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
