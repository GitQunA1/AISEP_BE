using AISEP.DAL.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Helpers
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(IOptions<SieveOptions> options) : base(options)
        {

        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            // Booking
            mapper.Property<Booking>(b => b.StartTime)
                .CanFilter()
                .CanSort();
            mapper.Property<Booking>(b => b.EndTime)
                .CanFilter()
                .CanSort();
            mapper.Property<Booking>(b => b.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<Booking>(b => b.Price)
                .CanFilter()
                .CanSort();

            // Advisor availability
            mapper.Property<AdvisorAvailability>(a => a.AdvisorId)
                .CanFilter()
                .CanSort();
            mapper.Property<AdvisorAvailability>(a => a.SlotDate)
                .CanFilter()
                .CanSort();
            mapper.Property<AdvisorAvailability>(a => a.StartTime)
                .CanFilter()
                .CanSort();
            mapper.Property<AdvisorAvailability>(a => a.EndTime)
                .CanFilter()
                .CanSort();
            mapper.Property<AdvisorAvailability>(a => a.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<AdvisorAvailability>(a => a.CreatedAt)
                .CanFilter()
                .CanSort();


            // Advisor
            mapper.Property<Advisor>(a => a.Rating)
                .CanFilter()
                .CanSort();
            // User
            mapper.Property<User>(u => u.Id)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.UserName)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.FullName)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.Email)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.PhoneNumber)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.Role)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.IsPremium)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.DateOfBirth)
                .CanFilter()
                .CanSort();
            mapper.Property<User>(u => u.CreatedAt)
                .CanFilter()
                .CanSort();

            // Transaction
            mapper.Property<Transaction>(t => t.TransactionId)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.UserId)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.Amount)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.Type)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.ReferenceType)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.ReferenceId)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.PaymentCode)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.SepayTransactionId)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.CompletedAt)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.CreatedAt)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.User.UserName)
                .CanFilter()
                .CanSort();
            mapper.Property<Transaction>(t => t.User.Email)
                .CanFilter()
                .CanSort();
            //Reviews
            mapper.Property<Review>(r => r.Rating)
                .CanFilter()
                .CanSort();
            mapper.Property<Review>(r => r.CreatedAt)
                .CanFilter()
                .CanSort();
            //ProjectFollower
            mapper.Property<ProjectFollower>(pf => pf.CreatedAt)
                .CanFilter()
                .CanSort();

            // Startup
            mapper.Property<Startup>(s => s.CompanyName)
                .CanFilter()
                .CanSort();
            mapper.Property<Startup>(s => s.CountryCity)
                .CanFilter()
                .CanSort();
            mapper.Property<Startup>(s => s.CreatedAt)
                .CanFilter()
                .CanSort();

            // Investor
            mapper.Property<Investor>(i => i.OrganizationName)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.InvestmentRegion)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.RiskTolerance)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.PreferredStageOption!.Value)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.InvestmentAmount)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.InvestmentDate)
                .CanFilter()
                .CanSort();

            // Project
            mapper.Property<Project>(p => p.ProjectId)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.ProjectName)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.StageOptionId)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.IndustryOptionId)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.StageOption!.Value)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.CreatedAt)
                .CanFilter()
                .CanSort();

            // Document
            mapper.Property<Document>(d => d.DocumentType)
                .CanFilter()
                .CanSort();
            mapper.Property<Document>(d => d.FileName)
                .CanFilter()
                .CanSort();
            mapper.Property<Document>(d => d.IsIpProtected)
                .CanFilter();
            mapper.Property<Document>(d => d.VerifiedAt)
                .CanFilter()
                .CanSort();

            // ConnectionRequest
            mapper.Property<ConnectionRequest>(cr => cr.ConnectionRequestId)
                .CanFilter()
                .CanSort();
            mapper.Property<ConnectionRequest>(cr => cr.InvestorId)
                .CanFilter()
                .CanSort();
            mapper.Property<ConnectionRequest>(cr => cr.ProjectId)
                .CanFilter()
                .CanSort();
            mapper.Property<ConnectionRequest>(cr => cr.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<ConnectionRequest>(cr => cr.ResponseDate)
                .CanFilter()
                .CanSort();

            // Deal
            mapper.Property<Deal>(d => d.DealId)
                .CanFilter()
                .CanSort();
            mapper.Property<Deal>(d => d.InvestorId)
                .CanFilter()
                .CanSort();
            mapper.Property<Deal>(d => d.ProjectId)
                .CanFilter()
                .CanSort();
            
            mapper.Property<Deal>(d => d.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<Deal>(d => d.DealDate)
                .CanFilter()
                .CanSort();
            mapper.Property<Deal>(d => d.IsCompleted)
                .CanFilter()
                .CanSort();

            // Subscription
            mapper.Property<Subscription>(s => s.SubscriptionId)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.PackageId)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.UserId)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.StartDate)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.EndDate)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.UsedAiRequests)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.UsedProjectViews)
                .CanFilter()
                .CanSort();
            mapper.Property<Subscription>(s => s.RemainingFreeBookings)
                .CanFilter()
                .CanSort();

            // Wallet
            mapper.Property<Wallet>(w => w.WalletId)
                .CanFilter()
                .CanSort();
            mapper.Property<Wallet>(w => w.AdvisorId)
                .CanFilter()
                .CanSort();
            mapper.Property<Wallet>(w => w.Balance)
                .CanFilter()
                .CanSort();
            mapper.Property<Wallet>(w => w.Currency)
                .CanFilter()
                .CanSort();
            mapper.Property<Wallet>(w => w.IsActive)
                .CanFilter()
                .CanSort();
            mapper.Property<Wallet>(w => w.Advisor.User.UserName)
                .CanFilter()
                .CanSort();
            mapper.Property<Wallet>(w => w.Advisor.User.Email)
                .CanFilter()
                .CanSort();

            // PostPr
            mapper.Property<PostPr>(p => p.PostPrId)
                .CanFilter()
                .CanSort();
            mapper.Property<PostPr>(p => p.DealId)
                .CanFilter()
                .CanSort();
            mapper.Property<PostPr>(p => p.Title)
                .CanFilter()
                .CanSort();
            mapper.Property<PostPr>(p => p.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<PostPr>(p => p.IsDelete)
                .CanFilter()
                .CanSort();
            mapper.Property<PostPr>(p => p.PublishedAt)
                .CanFilter()
                .CanSort();

            // ConsultingReport
            mapper.Property<ConsultingReport>(r => r.ConsultingReportId)
                .CanFilter()
                .CanSort();
            mapper.Property<ConsultingReport>(r => r.BookingId)
                .CanFilter()
                .CanSort();
            mapper.Property<ConsultingReport>(r => r.MeetingTime)
                .CanFilter()
                .CanSort();
            mapper.Property<ConsultingReport>(r => r.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<ConsultingReport>(r => r.RevisionCount)
                .CanFilter()
                .CanSort();
            mapper.Property<ConsultingReport>(r => r.IsPayoutProcessed)
                .CanFilter()
                .CanSort();
            mapper.Property<ConsultingReport>(r => r.CreatedAt)
                .CanFilter()
                .CanSort();

            // UserReport
            mapper.Property<UserReport>(r => r.UserReportId)
                .CanFilter()
                .CanSort();
            mapper.Property<UserReport>(r => r.ReporterId)
                .CanFilter()
                .CanSort();
            mapper.Property<UserReport>(r => r.BookingId)
                .CanFilter()
                .CanSort();
            mapper.Property<UserReport>(r => r.Category)
                .CanFilter()
                .CanSort();
            mapper.Property<UserReport>(r => r.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<UserReport>(r => r.ResolvedById)
                .CanFilter()
                .CanSort();
            mapper.Property<UserReport>(r => r.ResolvedAt)
                .CanFilter()
                .CanSort();
            mapper.Property<UserReport>(r => r.CreatedAt)
                .CanFilter()
                .CanSort();

            // Notification
            mapper.Property<Notification>(n => n.NotificationId)
                .CanFilter()
                .CanSort();
            mapper.Property<Notification>(n => n.UserId)
                .CanFilter()
                .CanSort();
            mapper.Property<Notification>(n => n.ReferenceId)
                .CanFilter()
                .CanSort();
            mapper.Property<Notification>(n => n.ReferenceType)
                .CanFilter()
                .CanSort();
            mapper.Property<Notification>(n => n.Type)
                .CanFilter()
                .CanSort();
            mapper.Property<Notification>(n => n.IsRead)
                .CanFilter()
                .CanSort();
            mapper.Property<Notification>(n => n.CreatedAt)
                .CanFilter()
                .CanSort();

            // Payout
            mapper.Property<Payout>(p => p.PayoutId)
                .CanFilter()
                .CanSort();
            mapper.Property<Payout>(p => p.PayoutGroupId)
                .CanFilter()
                .CanSort();
            mapper.Property<Payout>(p => p.WalletId)
                .CanFilter()
                .CanSort();
            mapper.Property<Payout>(p => p.PeriodFromDate)
                .CanFilter()
                .CanSort();
            mapper.Property<Payout>(p => p.PeriodToDate)
                .CanFilter()
                .CanSort();
            mapper.Property<Payout>(p => p.Amount)
                .CanFilter()
                .CanSort();
            mapper.Property<Payout>(p => p.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<Payout>(p => p.CreatedAt)
                .CanFilter()
                .CanSort();

            // PayoutGroup
            mapper.Property<PayoutGroup>(b => b.PayoutGroupId)
                .CanFilter()
                .CanSort();
            mapper.Property<PayoutGroup>(b => b.FromDate)
                .CanFilter()
                .CanSort();
            mapper.Property<PayoutGroup>(b => b.ToDate)
                .CanFilter()
                .CanSort();
            mapper.Property<PayoutGroup>(b => b.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<PayoutGroup>(b => b.CreatedAt)
                .CanFilter()
                .CanSort();

            // WalletTransaction
            mapper.Property<WalletTransaction>(t => t.WalletTransactionId)
                .CanFilter()
                .CanSort();
            mapper.Property<WalletTransaction>(t => t.WalletId)
                .CanFilter()
                .CanSort();
            mapper.Property<WalletTransaction>(t => t.PayoutId)
                .CanFilter()
                .CanSort();
            mapper.Property<WalletTransaction>(t => t.Amount)
                .CanFilter()
                .CanSort();
            mapper.Property<WalletTransaction>(t => t.Type)
                .CanFilter()
                .CanSort();
            mapper.Property<WalletTransaction>(t => t.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<WalletTransaction>(t => t.CreatedAt)
                .CanFilter()
                .CanSort();

            // SystemCommissionConfig
            mapper.Property<SystemCommissionConfig>(c => c.SystemCommissionConfigId)
                .CanFilter()
                .CanSort();
            mapper.Property<SystemCommissionConfig>(c => c.Percent)
                .CanFilter()
                .CanSort();
            mapper.Property<SystemCommissionConfig>(c => c.EffectiveFrom)
                .CanFilter()
                .CanSort();
            mapper.Property<SystemCommissionConfig>(c => c.EffectiveTo)
                .CanFilter()
                .CanSort();
            mapper.Property<SystemCommissionConfig>(c => c.IsActive)
                .CanFilter()
                .CanSort();
            mapper.Property<SystemCommissionConfig>(c => c.CreatedById)
                .CanFilter()
                .CanSort();
            mapper.Property<SystemCommissionConfig>(c => c.CreatedAt)
                .CanFilter()
                .CanSort();

            // IndustryOption
            mapper.Property<IndustryOption>(i => i.Id)
                .CanFilter()
                .CanSort();
            mapper.Property<IndustryOption>(i => i.Value)
                .CanFilter()
                .CanSort();
            mapper.Property<IndustryOption>(i => i.IsActive)
                .CanFilter()
                .CanSort();
            mapper.Property<IndustryOption>(i => i.CreatedAt)
                .CanFilter()
                .CanSort();
            mapper.Property<IndustryOption>(i => i.UpdatedAt)
                .CanFilter()
                .CanSort();

            // StageOption
            mapper.Property<StageOption>(s => s.Id)
                .CanFilter()
                .CanSort();
            mapper.Property<StageOption>(s => s.Value)
                .CanFilter()
                .CanSort();
            mapper.Property<StageOption>(s => s.IsActive)
                .CanFilter()
                .CanSort();
            mapper.Property<StageOption>(s => s.CreatedAt)
                .CanFilter()
                .CanSort();
            mapper.Property<StageOption>(s => s.UpdatedAt)
                .CanFilter()
                .CanSort();

            // FormValidationRule
            mapper.Property<FormValidationRule>(r => r.Id)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.FormKey)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.FieldKey)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.IsRequired)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.MinLength)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.MaxLength)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.CustomRegexPattern)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.MinValue)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.MaxValue)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.AllowedFileTypesJson)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.MaxFileSizeBytes)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.CreatedAt)
                .CanFilter()
                .CanSort();
            mapper.Property<FormValidationRule>(r => r.UpdatedAt)
                .CanFilter()
                .CanSort();

            return mapper;
        }
    }
}



