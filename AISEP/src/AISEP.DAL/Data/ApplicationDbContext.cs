using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;

namespace AISEP.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Startup> Startups { get; set; }
        public DbSet<Investor> Investors { get; set; }
        public DbSet<Advisor> Advisors { get; set; }
        public DbSet<AdvisorIndustry> AdvisorIndustries { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectAdvisorAssignment> ProjectAdvisorAssignments { get; set; }
        public DbSet<ProjectFollower> ProjectFollowers { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<StartupAIAnalysis> StartupAIAnalyses { get; set; }
        public DbSet<InvestorAIAnalysis> InvestorAIAnalyses { get; set; }
        public DbSet<ConnectionRequest> ConnectionRequests { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<NFTRecord> NFTRecords { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<PostPr> PostPrs { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ConsultingReport> ConsultingReports { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<UnlockedProject> UnlockedProjects { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<WithdrawRequest> WithdrawRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<AdvisorAvailability> AdvisorAvailabilities { get; set; }
        public DbSet<BookingSlot> BookingSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Identity tables ────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.Property(e => e.FullName).HasMaxLength(150);
                entity.Property(e => e.Role).HasConversion<string>().IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<IdentityRole<int>>().ToTable("roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("user_roles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("user_claims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("user_logins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("user_tokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("role_claims");

            // ── MODULE 1: PROFILES ─────────────────────────────────────────
            modelBuilder.Entity<Startup>(entity =>
            {
                entity.ToTable("startups");
                entity.HasKey(e => e.StartupId);
                entity.Property(e => e.CompanyName).HasMaxLength(255);
                entity.Property(e => e.LogoUrl).HasMaxLength(255);
                entity.Property(e => e.Founder).HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
                entity.Property(e => e.CountryCity).HasMaxLength(255);
                entity.Property(e => e.Website).HasMaxLength(255);
                entity.Property(e => e.Industry).HasConversion<string>();
                entity.Property(e => e.BusinessLicenseUrl).HasMaxLength(255);
                entity.Property(e => e.ApprovalStatus).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);

                entity.HasOne(s => s.User)
                    .WithOne(u => u.Startup)
                    .HasForeignKey<Startup>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(s => s.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(s => s.RejectedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Investor>(entity =>
            {
                entity.ToTable("investors");
                entity.HasKey(e => e.InvestorId);
                entity.Property(e => e.OrganizationName).HasMaxLength(255);
                entity.Property(e => e.InvestmentTaste).HasMaxLength(255);
                entity.Property(e => e.WalletAddress).HasMaxLength(255);
                entity.Property(e => e.InvestmentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RiskTolerance).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.InvestmentRegion).HasMaxLength(255);
                entity.Property(e => e.FocusIndustry).HasConversion<string>();
                entity.Property(e => e.PreferredStage).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.PreviousInvestments).HasMaxLength(255);
                entity.Property(e => e.IdentityDocumentUrl).HasMaxLength(255);
                entity.Property(e => e.ApprovalStatus).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);

                entity.HasOne(i => i.User)
                    .WithOne(u => u.Investor)
                    .HasForeignKey<Investor>(i => i.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(i => i.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(i => i.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(i => i.RejectedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Advisor>(entity =>
            {
                entity.ToTable("advisors");
                entity.HasKey(e => e.AdvisorId);
                entity.Property(e => e.Expertise).HasMaxLength(255);
                entity.Property(e => e.Rating).HasColumnType("decimal(3,2)");
                entity.Property(e => e.LanguagesSpoken).HasMaxLength(255);
                entity.Property(e => e.Location).HasMaxLength(255);
                entity.Property(e => e.ProfileImage).HasMaxLength(255);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApprovalStatus).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);

                entity.HasOne(a => a.User)
                    .WithOne(u => u.Advisor)
                    .HasForeignKey<Advisor>(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(a => a.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(a => a.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(a => a.RejectedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AdvisorIndustry>(entity =>
            {
                entity.ToTable("advisor_industries");
                entity.HasKey(x => new { x.AdvisorId, x.Industry });
                entity.Property(x => x.Industry).HasConversion<string>().HasMaxLength(50).IsRequired();

                entity.HasOne(x => x.Advisor)
                    .WithMany(a => a.AdvisorIndustries)
                    .HasForeignKey(x => x.AdvisorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── MODULE 2: PROJECTS & DOCUMENTS ────────────────────────────
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("projects");
                entity.HasKey(e => e.ProjectId);
                entity.Property(e => e.ProjectName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.ProjectImageUrl).HasMaxLength(500);
                entity.Property(e => e.DevelopmentStage).HasConversion<string>().HasMaxLength(50);
                entity.Property(e => e.Industry).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.MarketSize).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Revenue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(p => p.Startup)
                    .WithMany(s => s.Projects)
                    .HasForeignKey(p => p.StartupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProjectAdvisorAssignment>(entity =>
            {
                entity.ToTable("project_advisor_assignments");
                entity.HasKey(x => x.ProjectId);
                entity.Property(x => x.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(x => x.AdvisorId);

                entity.HasOne(x => x.Project)
                    .WithOne(p => p.ProjectAdvisorAssignment)
                    .HasForeignKey<ProjectAdvisorAssignment>(x => x.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Advisor)
                    .WithMany(a => a.ProjectAdvisorAssignments)
                    .HasForeignKey(x => x.AdvisorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("documents");
                entity.HasKey(e => e.DocumentId);
                entity.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FileUrl).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FileHash).HasMaxLength(255);
                entity.Property(e => e.BlockchainTxHash).HasMaxLength(255);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── MODULE 3: AI ANALYSES ──────────────────────────────────────
            modelBuilder.Entity<StartupAIAnalysis>(entity =>
            {
                entity.ToTable("project_ai_evaluations");
                entity.HasKey(e => e.EvaluationId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(a => a.Project)
                    .WithOne(p => p.StartupAIAnalysis)
                    .HasForeignKey<StartupAIAnalysis>(a => a.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InvestorAIAnalysis>(entity =>
            {
                entity.ToTable("investor_ai_analyses");
                entity.HasKey(e => e.AnalysisId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(a => a.Investor)
                    .WithMany(i => i.InvestorAIAnalyses)
                    .HasForeignKey(a => a.InvestorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Project)
                    .WithMany(p => p.InvestorAIAnalyses)
                    .HasForeignKey(a => a.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── MODULE 4: CONNECTIONS, DEALS & NFT ────────────────────────
            modelBuilder.Entity<ConnectionRequest>(entity =>
            {
                entity.ToTable("connection_requests");
                entity.HasKey(e => e.ConnectionRequestId);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

                entity.HasOne(cr => cr.Investor)
                    .WithMany(i => i.ConnectionRequests)
                    .HasForeignKey(cr => cr.InvestorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cr => cr.Project)
                    .WithMany(p => p.ConnectionRequests)
                    .HasForeignKey(cr => cr.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Deal>(entity =>
            {
                entity.ToTable("deals");
                entity.HasKey(e => e.DealId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.EquityPercentage).HasColumnType("decimal(5,2)");
                entity.Property(e => e.AdditionalTerms).HasColumnType("text");
                entity.Property(e => e.InvestorSignature).HasColumnType("text");
                entity.Property(e => e.StartupSignature).HasColumnType("text");
                entity.Property(e => e.DealDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ContractPdfUrl).HasMaxLength(500);

                entity.HasOne(d => d.Investor)
                    .WithMany(i => i.Deals)
                    .HasForeignKey(d => d.InvestorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Deals)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<NFTRecord>(entity =>
            {
                entity.ToTable("nft_records");
                entity.HasKey(e => e.NFTRecordId);
                entity.Property(e => e.TokenId).HasMaxLength(255).IsRequired();
                entity.Property(e => e.TxHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.OwnerWallet).HasMaxLength(255).IsRequired();
                entity.Property(e => e.ValidityStatus).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.PreviousOwnerWallet).HasMaxLength(255);
                entity.Property(e => e.MintedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(n => n.Deal)
                    .WithOne(d => d.NFTRecord)
                    .HasForeignKey<NFTRecord>(n => n.DealId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostPr>(entity =>
            {
                entity.ToTable("postprs");
                entity.HasKey(e => e.PostPrId);
                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(p => p.ConnectionRequest)
                    .WithMany(cr => cr.PostPrs)
                    .HasForeignKey(p => p.ConnectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── MODULE 5: ADVISORY, BOOKING & REVIEWS ─────────────────────
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("bookings");
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.Note).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(b => b.Advisor)
                    .WithMany(a => a.Bookings)
                    .HasForeignKey(b => b.AdvisorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Project)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(b => b.ProjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Customer)
                    .WithMany(u => u.CustomerBookings)
                    .HasForeignKey(b => b.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AdvisorAvailability>(entity =>
            {
                entity.ToTable("advisor_availabilities");
                entity.HasKey(e => e.AdvisorAvailabilityId);
                entity.Property(e => e.SlotDate).HasColumnType("date");
                entity.Property(e => e.StartTime).HasColumnType("time");
                entity.Property(e => e.EndTime).HasColumnType("time");
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => new { e.AdvisorId, e.SlotDate, e.StartTime, e.EndTime }).IsUnique();

                entity.HasOne(a => a.Advisor)
                    .WithMany(a => a.Availabilities)
                    .HasForeignKey(a => a.AdvisorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BookingSlot>(entity =>
            {
                entity.ToTable("booking_slots");
                entity.HasKey(e => e.BookingSlotId);
                entity.HasIndex(e => new { e.BookingId, e.AdvisorAvailabilityId }).IsUnique();
                entity.HasIndex(e => e.AdvisorAvailabilityId).IsUnique();

                entity.HasOne(bs => bs.Booking)
                    .WithMany(b => b.BookingSlots)
                    .HasForeignKey(bs => bs.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bs => bs.AdvisorAvailability)
                    .WithMany(a => a.BookingSlots)
                    .HasForeignKey(bs => bs.AdvisorAvailabilityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.ToTable("chat_sessions", table =>
                {
                    table.HasCheckConstraint(
                        "CK_chat_sessions_context",
                        "(\"BookingId\" IS NOT NULL AND \"ConnectionRequestId\" IS NULL) OR (\"BookingId\" IS NULL AND \"ConnectionRequestId\" IS NOT NULL)");
                });
                entity.HasKey(e => e.ChatSessionId);
                entity.Property(e => e.StartTime).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(cs => cs.Booking)
                    .WithOne(b => b.ChatSession)
                    .HasForeignKey<ChatSession>(cs => cs.BookingId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cs => cs.ConnectionRequest)
                    .WithOne(cr => cr.ChatSession)
                    .HasForeignKey<ChatSession>(cs => cs.ConnectionRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("chat_messages");
                entity.HasKey(e => e.ChatMessageId);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.SentAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(cm => cm.ChatSession)
                    .WithMany(cs => cs.ChatMessages)
                    .HasForeignKey(cm => cm.ChatSessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cm => cm.Sender)
                    .WithMany(u => u.ChatMessages)
                    .HasForeignKey(cm => cm.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ConsultingReport>(entity =>
            {
                entity.ToTable("consulting_reports");
                entity.HasKey(e => e.ConsultingReportId);
                entity.Property(e => e.MeetingTitle).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Location).HasMaxLength(255);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.RevisionRequestReason).HasMaxLength(2000);
                entity.Property(e => e.AdvisorPayoutAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(cr => cr.Booking)
                    .WithOne(b => b.ConsultingReport)
                    .HasForeignKey<ConsultingReport>(cr => cr.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");
                entity.HasKey(e => e.ReviewId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(r => r.Advisor)
                    .WithMany(a => a.Reviews)
                    .HasForeignKey(r => r.AdvisorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Reviewer)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Booking)
                    .WithOne(b => b.Review)
                    .HasForeignKey<Review>(r => r.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── MODULE 6: FINANCE & SUBSCRIPTIONS ─────────────────────────
            modelBuilder.Entity<Package>(entity =>
            {
                entity.ToTable("packages");
                entity.HasKey(e => e.PackageId);
                entity.Property(e => e.PackageName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.TargetRole).HasConversion<string>().HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("subscriptions");
                entity.HasKey(e => e.SubscriptionId);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

                entity.HasOne(s => s.Package)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(s => s.PackageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.User)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UnlockedProject>(entity =>
            {
                entity.ToTable("unlocked_projects");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnlockedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => new { e.UserId, e.ProjectId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UnlockedProjects)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Project)
                    .WithMany(p => p.UnlockedProjects)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("wallets");
                entity.HasKey(e => e.WalletId);
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();

                entity.HasOne(w => w.Advisor)
                    .WithOne(a => a.Wallet)
                    .HasForeignKey<Wallet>(w => w.AdvisorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.ToTable("wallet_transactions");
                entity.HasKey(e => e.WalletTransactionId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(wt => wt.Wallet)
                    .WithMany(w => w.WalletTransactions)
                    .HasForeignKey(wt => wt.WalletId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WithdrawRequest>(entity =>
            {
                entity.ToTable("withdraw_requests");
                entity.HasKey(e => e.WithdrawRequestId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.BankName).HasMaxLength(255);
                entity.Property(e => e.BankAccount).HasMaxLength(255);
                entity.Property(e => e.ProofImageUrl).HasMaxLength(255);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.RequestedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(wr => wr.Wallet)
                    .WithMany(w => w.WithdrawRequests)
                    .HasForeignKey(wr => wr.WalletId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");
                entity.HasKey(e => e.TransactionId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Reference fields
                entity.Property(e => e.ReferenceType).HasMaxLength(50);
                entity.Property(e => e.ReferenceId);

                // SePay fields
                entity.Property(e => e.PaymentCode).HasMaxLength(50);
                entity.HasIndex(e => e.PaymentCode).IsUnique().HasFilter("\"PaymentCode\" IS NOT NULL");
                entity.Property(e => e.SepayTransactionId).HasMaxLength(255);
                entity.Property(e => e.PaymentContent);
                entity.Property(e => e.CompletedAt);

                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── MODULE 7: SYSTEM, LOGS & UTILS ────────────────────────────
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.ReferenceType).HasMaxLength(50);
                entity.Property(e => e.ReferenceId);
                entity.Property(e => e.Title).HasMaxLength(255);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.IsRead).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ActionLog>(entity =>
            {
                entity.ToTable("action_logs");
                entity.HasKey(e => e.ActionLogId);
                entity.Property(e => e.ActionType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(a => a.Actor)
                    .WithMany(u => u.ActionLogs)
                    .HasForeignKey(a => a.ActorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserReport>(entity =>
            {
                entity.ToTable("user_reports");
                entity.HasKey(e => e.UserReportId);
                entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.EvidenceUrl).HasMaxLength(255);
                entity.Property(e => e.EvidenceImageUrls);
                entity.Property(e => e.VideoEvidenceUrl).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(r => r.Reporter)
                    .WithMany(u => u.ReportsMade)
                    .HasForeignKey(r => r.ReporterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReportedUser)
                    .WithMany(u => u.ReportsReceived)
                    .HasForeignKey(r => r.ReportedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.RefreshTokenId);
                entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.CreatedByIp).HasMaxLength(50);
                entity.Property(e => e.RevokedByIp).HasMaxLength(50);
                entity.Property(e => e.ReplacedByToken).HasMaxLength(500);

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token);
            });

            modelBuilder.Entity<ProjectFollower>(entity =>
            {
                entity.ToTable("project_followers");
                entity.HasKey(pf => pf.ProjectFollowerId);

                entity.HasOne(pf => pf.User)
                    .WithMany(u => u.FollowedProjects)
                    .HasForeignKey(pf => pf.FollowerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pf => pf.Project)
                    .WithMany(p => p.Followers)
                    .HasForeignKey(pf => pf.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(pf => pf.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
               
            });
        }
    }
}
