using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AISEP.DAL.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await context.Database.MigrateAsync();

            await SeedUsersAsync(userManager, context);
            await SeedAdvisorsAsync(context);
            await SeedInvestorsAsync(context);
            await SeedStartupsAsync(context);
            await SeedPackagesAsync(context);
            await SeedWalletsAsync(context);
            await SeedProjectsAsync(context);
            await SeedBookingsAsync(context);
            await SeedConnectionRequestsAsync(context);
            await SeedReviewsAsync(context);
            await SeedNotificationsAsync(context);
            await SeedSubscriptionsAsync(context);
            await SeedStartupFollowersAsync(context);
        }

        // =============================================
        // 1. USERS
        // =============================================
        private static async Task SeedUsersAsync(UserManager<User> userManager, ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var users = new[]
            {
                new { Email = "admin@aisep.com",     Name = "AdminAISEP",        Role = UserRole.Admin,    Password = "Admin@123" },
                new { Email = "advisor1@aisep.com",  Name = "NguyenVanAdvisor",  Role = UserRole.Advisor,  Password = "Advisor@123" },
                new { Email = "advisor2@aisep.com",  Name = "TranThiAdvisor",    Role = UserRole.Advisor,  Password = "Advisor@123" },
                new { Email = "investor1@aisep.com", Name = "LeVanInvestor",     Role = UserRole.Investor, Password = "Investor@123" },
                new { Email = "investor2@aisep.com", Name = "PhamThiInvestor",   Role = UserRole.Investor, Password = "Investor@123" },
                new { Email = "startup1@aisep.com",  Name = "TechStartVN",       Role = UserRole.Startup,  Password = "Startup@123" },
                new { Email = "startup2@aisep.com",  Name = "GreenFarmTech",     Role = UserRole.Startup,  Password = "Startup@123" },
                new { Email = "startup3@aisep.com",  Name = "EduTechSolutions",  Role = UserRole.Startup,  Password = "Startup@123" },
                new { Email = "staff1@aisep.com",    Name = "HoangVanStaff",     Role = UserRole.Staff,    Password = "Staff@123" },
            };

            foreach (var u in users)
            {
                var user = new User
                {
                    UserName = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    Status = UserStatus.Active,
                    //IsVerified     = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, u.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"[Seeder] ❌ Lỗi tạo user {u.Email}: {errors}");
                }
                else
                {
                    Console.WriteLine($"[Seeder] ✅ Tạo user: {u.Email}");
                }
            }
        }

        // =============================================
        // 2. ADVISORS
        // =============================================
        private static async Task SeedAdvisorsAsync(ApplicationDbContext context)
        {
            if (await context.Advisors.AnyAsync()) return;

            var advisorUsers = await context.Users
                .Where(u => u.Role == UserRole.Advisor)
                .ToListAsync();

            if (advisorUsers.Count < 2)
            {
                Console.WriteLine("[Seeder] ⚠️ Không đủ Advisor users, bỏ qua.");
                return;
            }

            var advisors = new List<Advisor>
            {
                new Advisor
                {
                    UserId             = advisorUsers[0].Id,
                    Bio                = "Chuyên gia tư vấn startup công nghệ với hơn 10 năm kinh nghiệm",
                    Expertise          = "FinTech, SaaS, AI/ML",
                    Certifications     = "CFA, PMP",
                    PreviousExperience = "Ex-CTO tại FPT Software, Co-founder tại TechVN",
                    Rating             = 4.8m,
                    HourlyRate         = 500000m,
                    LanguagesSpoken    = "Vietnamese, English",
                    Location           = "Ho Chi Minh City",
                    ProfileImage       = "https://example.com/advisor1.jpg",
                    ApprovalStatus     = ApprovalStatus.Approved
                },
                new Advisor
                {
                    UserId             = advisorUsers[1].Id,
                    Bio                = "Chuyên gia tư vấn đầu tư và phát triển kinh doanh",
                    Expertise          = "AgriTech, GreenTech, Sustainability",
                    Certifications     = "MBA, CPA",
                    PreviousExperience = "Investment Manager tại VinaCapital, Partner tại MeKong Capital",
                    Rating             = 4.6m,
                    HourlyRate         = 400000m,
                    LanguagesSpoken    = "Vietnamese, English, French",
                    Location           = "Ha Noi",
                    ProfileImage       = "https://example.com/advisor2.jpg",
                    ApprovalStatus     = ApprovalStatus.Approved
                }
            };

            await context.Advisors.AddRangeAsync(advisors);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Advisors xong.");
        }

        // =============================================
        // 3. INVESTORS
        // =============================================
        private static async Task SeedInvestorsAsync(ApplicationDbContext context)
        {
            if (await context.Investors.AnyAsync()) return;

            var investorUsers = await context.Users
                .Where(u => u.Role == UserRole.Investor)
                .ToListAsync();

            if (investorUsers.Count < 2)
            {
                Console.WriteLine("[Seeder] ⚠️ Không đủ Investor users, bỏ qua.");
                return;
            }

            var investors = new List<Investor>
            {
                new Investor
                {
                    UserId              = investorUsers[0].Id,
                    OrganizationName    = "VN Tech Ventures",
                    InvestmentTaste     = "Early stage B2B SaaS, AI/ML startups",
                    WalletAddress       = "0x1234567890abcdef",
                    InvestmentAmount    = 500000m,
                    InvestmentDate      = DateTime.UtcNow.AddMonths(-6),
                    RiskTolerance       = RiskTolerance.High,
                    InvestmentRegion    = "Southeast Asia",
                    FocusIndustry       = Industry.Fintech,
                    PreferredStage      = PreferredStage.MVP,
                    PreviousInvestments = "StartupX (exit 2x), TechY (active)",
                    ApprovalStatus      = ApprovalStatus.Approved
                },
                new Investor
                {
                    UserId              = investorUsers[1].Id,
                    OrganizationName    = "GreenGrowth Fund",
                    InvestmentTaste     = "Sustainable agriculture, green energy",
                    WalletAddress       = "0xabcdef1234567890",
                    InvestmentAmount    = 300000m,
                    InvestmentDate      = DateTime.UtcNow.AddMonths(-3),
                    RiskTolerance       = RiskTolerance.Medium,
                    InvestmentRegion    = "Vietnam, Cambodia",
                    FocusIndustry       = Industry.Agritech,
                    PreferredStage      = PreferredStage.Growth,
                    PreviousInvestments = "FarmTech VN (active), EcoEnergy (exit 3x)",
                    ApprovalStatus      = ApprovalStatus.Approved
                }
            };

            await context.Investors.AddRangeAsync(investors);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Investors xong.");
        }

        // =============================================
        // 4. STARTUPS
        // =============================================
        private static async Task SeedStartupsAsync(ApplicationDbContext context)
        {
            if (await context.Startups.AnyAsync()) return;

            var startupUsers = await context.Users
                .Where(u => u.Role == UserRole.Startup)
                .ToListAsync();

            if (startupUsers.Count < 3)
            {
                Console.WriteLine("[Seeder] ⚠️ Không đủ Startup users, bỏ qua.");
                return;
            }

            var startups = new List<Startup>
            {
                new Startup
                {
                    UserId         = startupUsers[0].Id,
                    CompanyName    = "TechStart VN",
                    Founder        = "Nguyen Van A",
                    Email          = "techstart@gmail.com",
                    PhoneNumber    = "0901234567",
                    CountryCity    = "Ho Chi Minh City, Vietnam",
                    Website        = "https://techstart.vn",
                    Industry       = Industry.Fintech,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt      = DateTime.UtcNow.AddMonths(-3)
                },
                new Startup
                {
                    UserId         = startupUsers[1].Id,
                    CompanyName    = "GreenFarm Tech",
                    Founder        = "Pham Thi B",
                    Email          = "greenfarm@gmail.com",
                    PhoneNumber    = "0912345678",
                    CountryCity    = "Can Tho, Vietnam",
                    Website        = "https://greenfarm.tech",
                    Industry       = Industry.Agritech,
                    ApprovalStatus = ApprovalStatus.Approved,
                    CreatedAt      = DateTime.UtcNow.AddMonths(-2)
                },
                new Startup
                {
                    UserId         = startupUsers[2].Id,
                    CompanyName    = "EduTech Solutions",
                    Founder        = "Vo Van C",
                    Email          = "edutech@gmail.com",
                    PhoneNumber    = "0923456789",
                    CountryCity    = "Da Nang, Vietnam",
                    Website        = "https://edutech.vn",
                    Industry       = Industry.Edtech,
                    ApprovalStatus = ApprovalStatus.Pending,
                    CreatedAt      = DateTime.UtcNow.AddMonths(-1)
                }
            };

            await context.Startups.AddRangeAsync(startups);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Startups xong.");
        }

        // =============================================
        // 5. PACKAGES
        // =============================================
        private static async Task SeedPackagesAsync(ApplicationDbContext context)
        {
            if (await context.Packages.AnyAsync()) return;

            var packages = new List<Package>
            {
                new Package { PackageName = "Basic",      Description = "Gói cơ bản - Phù hợp cho startup mới",               Price = 99000m,  DurationMonths = 1 },
                new Package { PackageName = "Pro",        Description = "Gói Pro - Đầy đủ tính năng cho startup tăng trưởng",  Price = 299000m, DurationMonths = 1 },
                new Package { PackageName = "Enterprise", Description = "Gói doanh nghiệp - Cho startup giai đoạn scale",       Price = 999000m, DurationMonths = 1 }
            };

            await context.Packages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Packages xong.");
        }

        // =============================================
        // 6. WALLETS (chỉ dành cho Advisor)
        // =============================================
        private static async Task SeedWalletsAsync(ApplicationDbContext context)
        {
            if (await context.Wallets.AnyAsync()) return;

            var advisors = await context.Advisors.ToListAsync();
            if (!advisors.Any()) return;

            var wallets = advisors.Select(a => new Wallet
            {
                AdvisorId = a.AdvisorId,
                Balance = 5000000m,
                Currency = "VND",
                IsActive = true
            }).ToList();

            await context.Wallets.AddRangeAsync(wallets);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Wallets xong.");
        }

        // =============================================
        // 7. PROJECTS
        // =============================================
        private static async Task SeedProjectsAsync(ApplicationDbContext context)
        {
            if (await context.Projects.AnyAsync()) return;

            var startups = await context.Startups.ToListAsync();
            if (startups.Count < 3)
            {
                Console.WriteLine("[Seeder] ⚠️ Không đủ Startups, bỏ qua SeedProjects.");
                return;
            }

            var projects = new List<Project>
            {
                new Project
                {
                    StartupId              = startups[0].StartupId,
                    ProjectName            = "AISEP Payment Module",
                    ShortDescription       = "Module thanh toán thông minh cho SMEs",
                    DevelopmentStage       = DevelopmentStage.MVP,
                    ProblemStatement       = "Khó khăn trong thanh toán số cho doanh nghiệp nhỏ",
                    SolutionDescription    = "Nền tảng thanh toán số tích hợp AI",
                    TargetCustomers        = "SMEs tại Việt Nam",
                    UniqueValueProposition = "Phí thấp hơn 60% so với thị trường",
                    MarketSize             = 5000000000m,
                    BusinessModel          = "SaaS subscription + transaction fees",
                    Revenue                = 50000m,
                    Competitors            = "MoMo, ZaloPay",
                    TeamMembers            = "Nguyen Van A (CEO), Tran Thi B (CTO), Le Van C (CFO)",
                    KeySkills              = "FinTech, Blockchain, Mobile Dev",
                    TeamExperience         = "10+ years combined experience",
                    Status                 = ProjectStatus.Approved,
                    CreatedAt              = DateTime.UtcNow.AddDays(-20),
                    //PublishedAt            = DateTime.UtcNow.AddDays(-10)
                },
                new Project
                {
                    StartupId              = startups[1].StartupId,
                    ProjectName            = "Smart Farm IoT",
                    ShortDescription       = "Hệ thống IoT quản lý nông trại thông minh",
                    DevelopmentStage       = DevelopmentStage.Growth,
                    ProblemStatement       = "Nông dân thiếu công cụ quản lý và bán hàng hiệu quả",
                    SolutionDescription    = "App quản lý nông trại thông minh tích hợp IoT",
                    TargetCustomers        = "Nông dân và hợp tác xã nông nghiệp",
                    UniqueValueProposition = "Kết nối nông dân trực tiếp với người mua, tăng thu nhập 40%",
                    MarketSize             = 2000000000m,
                    BusinessModel          = "Commission + SaaS",
                    Revenue                = 120000m,
                    Competitors            = "eFarm, Agrimart",
                    TeamMembers            = "Pham Thi B (CEO), Hoang Van D (CTO)",
                    KeySkills              = "AgriTech, IoT, Mobile Dev",
                    TeamExperience         = "8+ years in agriculture and technology",
                    Status                 = ProjectStatus.Approved,
                    CreatedAt              = DateTime.UtcNow.AddDays(-15),
                    //PublishedAt            = DateTime.UtcNow.AddDays(-5)
                },
                new Project
                {
                    StartupId              = startups[2].StartupId,
                    ProjectName            = "EduConnect Platform",
                    ShortDescription       = "Nền tảng kết nối giáo viên và học sinh",
                    DevelopmentStage       = DevelopmentStage.Idea,
                    ProblemStatement       = "Học sinh thiếu giáo viên giỏi tại vùng nông thôn",
                    SolutionDescription    = "Nền tảng học trực tuyến kết nối giáo viên giỏi toàn quốc",
                    TargetCustomers        = "Học sinh K-12 tại nông thôn",
                    UniqueValueProposition = "Chi phí thấp, chất lượng cao nhờ AI personalization",
                    MarketSize             = 3000000000m,
                    BusinessModel          = "Freemium + Premium subscription",
                    Revenue                = 0m,
                    Competitors            = "Hocmai, Topica",
                    TeamMembers            = "Vo Van C (CEO), Nguyen Thi D (CTO)",
                    KeySkills              = "EdTech, AI, UX Design",
                    TeamExperience         = "5+ years in education and technology",
                    Status                 = ProjectStatus.Draft,
                    CreatedAt              = DateTime.UtcNow.AddDays(-5)
                }
            };

            await context.Projects.AddRangeAsync(projects);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Projects xong.");
        }

        // =============================================
        // 8. BOOKINGS
        // =============================================
        private static async Task SeedBookingsAsync(ApplicationDbContext context)
        {
            if (await context.Bookings.AnyAsync()) return;

            var advisor = await context.Advisors.FirstOrDefaultAsync();
            var customers = await context.Users
                .Where(u => u.Role == UserRole.Startup)
                .ToListAsync();

            if (advisor == null || customers.Count < 2) return;

            var bookings = new List<Booking>
            {
                new Booking
                {
                    AdvisorId  = advisor.AdvisorId,
                    CustomerId = customers[0].Id,
                    StartTime  = DateTime.UtcNow.AddDays(-5),
                    EndTime    = DateTime.UtcNow.AddDays(-5).AddHours(1),
                    Price      = 500000m,
                    Status     = BookingStatus.Completed
                },
                new Booking
                {
                    AdvisorId  = advisor.AdvisorId,
                    CustomerId = customers[1].Id,
                    StartTime  = DateTime.UtcNow.AddDays(1),
                    EndTime    = DateTime.UtcNow.AddDays(1).AddHours(1),
                    Price      = 500000m,
                    Status     = BookingStatus.Confirmed
                },
                new Booking
                {
                    AdvisorId  = advisor.AdvisorId,
                    CustomerId = customers[0].Id,
                    StartTime  = DateTime.UtcNow.AddDays(3),
                    EndTime    = DateTime.UtcNow.AddDays(3).AddHours(1),
                    Price      = 500000m,
                    Status     = BookingStatus.Pending
                }
            };

            await context.Bookings.AddRangeAsync(bookings);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Bookings xong.");
        }

        // =============================================
        // 9. CONNECTION REQUESTS (Investor → Project)
        // =============================================
        private static async Task SeedConnectionRequestsAsync(ApplicationDbContext context)
        {
            if (await context.ConnectionRequests.AnyAsync()) return;

            var investor = await context.Investors.FirstOrDefaultAsync();
            var projects = await context.Projects
                .Where(p => p.Status == ProjectStatus.Approved)
                .ToListAsync();

            if (investor == null || projects.Count < 2) return;

            var requests = new List<ConnectionRequest>
            {
                new ConnectionRequest
                {
                    InvestorId      = investor.InvestorId,
                    ProjectId       = projects[0].ProjectId,
                    Status          = ConnectionRequestStatus.Accepted,
                    //RequestDate     = DateTime.UtcNow.AddDays(-10),
                    ResponseDate    = DateTime.UtcNow.AddDays(-8),
                    Message         = "Chúng tôi rất quan tâm đến giải pháp thanh toán của các bạn",
                    //ResponseMessage = "Cảm ơn bạn, chúng tôi rất vui được hợp tác!",
                    //CreatedAt       = DateTime.UtcNow.AddDays(-10)
                },
                new ConnectionRequest
                {
                    InvestorId  = investor.InvestorId,
                    ProjectId   = projects[1].ProjectId,
                    Status      = ConnectionRequestStatus.Pending,
                    //RequestDate = DateTime.UtcNow.AddDays(-2),
                    Message     = "AgriTech là lĩnh vực chúng tôi đang tập trung đầu tư",
                    //CreatedAt   = DateTime.UtcNow.AddDays(-2)
                }
            };

            await context.ConnectionRequests.AddRangeAsync(requests);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed ConnectionRequests xong.");
        }

        // =============================================
        // 10. REVIEWS
        // =============================================
        private static async Task SeedReviewsAsync(ApplicationDbContext context)
        {
            if (await context.Reviews.AnyAsync()) return;

            var completedBooking = await context.Bookings
                .FirstOrDefaultAsync(b => b.Status == BookingStatus.Completed);

            if (completedBooking == null) return;

            var review = new Review
            {
                AdvisorId = completedBooking.AdvisorId,
                ReviewerId = completedBooking.CustomerId,
                BookingId = completedBooking.BookingId,
                Rating = 5,
                ReviewContent = "Advisor rất chuyên nghiệp, giúp chúng tôi định hình được chiến lược kinh doanh rõ ràng",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            await context.Reviews.AddAsync(review);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Reviews xong.");
        }

        // =============================================
        // 11. NOTIFICATIONS
        // =============================================
        private static async Task SeedNotificationsAsync(ApplicationDbContext context)
        {
            if (await context.Notifications.AnyAsync()) return;

            var users = await context.Users.Take(5).ToListAsync();

            var notifications = users.SelectMany(u => new[]
            {
                new Notification
                {
                    UserId    = u.Id,
                    Title     = "Chào mừng",
                    Message   = "Chào mừng bạn đến với AISEP Platform!",
                    Type      = "System",
                    IsRead    = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    UserId    = u.Id,
                    Title     = "Xác minh thành công",
                    Message   = "Hồ sơ của bạn đã được xác minh thành công",
                    Type      = "Profile",
                    IsRead    = true,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            }).ToList();

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Notifications xong.");
        }

        // =============================================
        // 12. SUBSCRIPTIONS
        // =============================================
        private static async Task SeedSubscriptionsAsync(ApplicationDbContext context)
        {
            if (await context.Subscriptions.AnyAsync()) return;

            var package = await context.Packages.FirstOrDefaultAsync(p => p.PackageName == "Pro");
            var startupUsers = await context.Users
                .Where(u => u.Role == UserRole.Startup)
                .ToListAsync();

            if (package == null || !startupUsers.Any()) return;

            var subscriptions = startupUsers.Take(2).Select(u => new Subscription
            {
                PackageId = package.PackageId,
                UserId = u.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = SubscriptionStatus.Active
            }).ToList();

            await context.Subscriptions.AddRangeAsync(subscriptions);
            await context.SaveChangesAsync();
            Console.WriteLine("[Seeder] ✅ Seed Subscriptions xong.");
        }

        // =============================================
        // 13. STARTUP FOLLOWERS
        // =============================================
        private static async Task SeedStartupFollowersAsync(ApplicationDbContext context)
        {
            if (await context.StartupFollowers.AnyAsync()) return;

            var investorUsers = await context.Users.Where(u => u.Role == UserRole.Investor).ToListAsync();
            var advisorUsers = await context.Users.Where(u => u.Role == UserRole.Advisor).ToListAsync();
            var staffUsers = await context.Users.Where(u => u.Role == UserRole.Staff).ToListAsync();
            var startups = await context.Startups.ToListAsync();

            if (!startups.Any()) return;

            var followers = new List<StartupFollower>();

            if (investorUsers.Count > 0)
            {
                if (startups.Count > 0)
                    followers.Add(new StartupFollower { FollowerId = investorUsers[0].Id, FollowedId = startups[0].StartupId, CreatedAt = DateTime.UtcNow.AddDays(-15) });
                if (startups.Count > 2)
                    followers.Add(new StartupFollower { FollowerId = investorUsers[0].Id, FollowedId = startups[2].StartupId, CreatedAt = DateTime.UtcNow.AddDays(-10) });
            }

            if (investorUsers.Count > 1)
                foreach (var s in startups)
                    followers.Add(new StartupFollower { FollowerId = investorUsers[1].Id, FollowedId = s.StartupId, CreatedAt = DateTime.UtcNow.AddDays(-7) });

            if (advisorUsers.Count > 0)
            {
                if (startups.Count > 0)
                    followers.Add(new StartupFollower { FollowerId = advisorUsers[0].Id, FollowedId = startups[0].StartupId, CreatedAt = DateTime.UtcNow.AddDays(-5) });
                if (startups.Count > 1)
                    followers.Add(new StartupFollower { FollowerId = advisorUsers[0].Id, FollowedId = startups[1].StartupId, CreatedAt = DateTime.UtcNow.AddDays(-5) });
            }

            if (advisorUsers.Count > 1 && startups.Count > 2)
                followers.Add(new StartupFollower { FollowerId = advisorUsers[1].Id, FollowedId = startups[2].StartupId, CreatedAt = DateTime.UtcNow.AddDays(-3) });

            if (staffUsers.Count > 0 && startups.Count > 0)
                followers.Add(new StartupFollower { FollowerId = staffUsers[0].Id, FollowedId = startups[0].StartupId, CreatedAt = DateTime.UtcNow.AddDays(-2) });

            if (followers.Any())
            {
                await context.StartupFollowers.AddRangeAsync(followers);
                await context.SaveChangesAsync();
                Console.WriteLine($"[Seeder] ✅ Seed {followers.Count} StartupFollowers xong.");
            }
        }
    }
}
