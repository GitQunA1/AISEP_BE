//using AISEP.Models;
//using AISEP.Models.Entities;
//using AISEP.Models.Enums;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//namespace AISEP.Data
//{
//    public static class DataSeeder
//    {
//        public static async Task SeedAsync(IServiceProvider serviceProvider)
//        {
//            using var scope = serviceProvider.CreateScope();
//            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

//            await context.Database.MigrateAsync();

//            // Seed theo thứ tự dependency
//            await SeedUsersAsync(userManager, context);
//            await SeedAdvisorsAsync(context);
//            await SeedInvestorsAsync(context);
//            await SeedStartupsAsync(context);
//            await SeedPackagesAsync(context);
//            await SeedWalletsAsync(context);
//            await SeedBookingsAsync(context);
//            await SeedProjectsAsync(context);
//            await SeedConnectionRequestsAsync(context);
//            await SeedReviewsAsync(context);
//            await SeedNotificationsAsync(context);
//            await SeedSubscriptionsAsync(context);
//        }

//        // =============================================
//        // 1. USERS
//        // =============================================
//        private static async Task SeedUsersAsync(UserManager<User> userManager, ApplicationDbContext context)
//        {
//            if (await context.Users.AnyAsync()) return;

//            var users = new[]
//            {
//                // Admin
//                new { Email = "admin@aisep.com",     Name = "AdminAISEP",        Role = UserRole.Admin,    Password = "Admin@123" },
//                // Advisors
//                new { Email = "advisor1@aisep.com",  Name = "NguyenVanAdvisor",  Role = UserRole.Advisor,  Password = "Advisor@123" },
//                new { Email = "advisor2@aisep.com",  Name = "TranThiAdvisor",    Role = UserRole.Advisor,  Password = "Advisor@123" },
//                // Investors
//                new { Email = "investor1@aisep.com", Name = "LeVanInvestor",     Role = UserRole.Investor, Password = "Investor@123" },
//                new { Email = "investor2@aisep.com", Name = "PhamThiInvestor",   Role = UserRole.Investor, Password = "Investor@123" },
//                // Startups
//                new { Email = "startup1@aisep.com",  Name = "TechStartVN",       Role = UserRole.Startup,  Password = "Startup@123" },
//                new { Email = "startup2@aisep.com",  Name = "GreenFarmTech",     Role = UserRole.Startup,  Password = "Startup@123" },
//                new { Email = "startup3@aisep.com",  Name = "EduTechSolutions",  Role = UserRole.Startup,  Password = "Startup@123" },
//                // Staff
//                new { Email = "staff1@aisep.com",    Name = "HoangVanStaff",     Role = UserRole.Staff,    Password = "Staff@123" },
//            };

//            foreach (var u in users)
//            {
//                var user = new User
//                {
//                    Id             = Guid.NewGuid(),
//                    UserName       = u.Name,
//                    Email          = u.Email,
//                    Role           = u.Role,
//                    Status         = UserStatus.Active,
//                    IsVerified     = true,
//                    CreatedAt      = DateTime.UtcNow,
//                    EmailConfirmed = true
//                };

//                var result = await userManager.CreateAsync(user, u.Password);
//                if (!result.Succeeded)
//                {
//                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//                    Console.WriteLine($"[Seeder] Lỗi tạo user {u.Email}: {errors}");
//                }
//                else
//                {
//                    Console.WriteLine($"[Seeder] ✅ Tạo user thành công: {u.Email}");
//                }
//            }
//        }

//        // =============================================
//        // 2. ADVISORS
//        // =============================================
//        private static async Task SeedAdvisorsAsync(ApplicationDbContext context)
//        {
//            if (await context.Advisors.AnyAsync()) return;

//            var advisorUsers = await context.Users
//                .Where(u => u.Role == UserRole.Advisor)
//                .ToListAsync();

//            // Guard: không có user Advisor nào thì bỏ qua
//            if (advisorUsers.Count < 2)
//            {
//                Console.WriteLine("[Seeder] Không đủ Advisor users, bỏ qua SeedAdvisors");
//                return;
//            }

//            var advisors = new List<Advisor>
//            {
//                new Advisor
//                {
//                    Id                 = Guid.NewGuid(),
//                    UserId             = advisorUsers[0].Id,
//                    Bio                = "Chuyên gia tư vấn startup công nghệ với hơn 10 năm kinh nghiệm",
//                    Expertise          = "FinTech, SaaS, AI/ML",
//                    Certifications     = "CFA, PMP",
//                    PreviousExperience = "Ex-CTO tại FPT Software, Co-founder tại TechVN",
//                    Rating             = 4.8m,
//                    LanguagesSpoken    = "Vietnamese, English",
//                    Location           = "Ho Chi Minh City",
//                    ProfileImage       = "https://example.com/advisor1.jpg"
//                },
//                new Advisor
//                {
//                    Id                 = Guid.NewGuid(),
//                    UserId             = advisorUsers[1].Id,
//                    Bio                = "Chuyên gia tư vấn đầu tư và phát triển kinh doanh",
//                    Expertise          = "AgriTech, GreenTech, Sustainability",
//                    Certifications     = "MBA, CPA",
//                    PreviousExperience = "Investment Manager tại VinaCapital, Partner tại MeKong Capital",
//                    Rating             = 4.6m,
//                    LanguagesSpoken    = "Vietnamese, English, French",
//                    Location           = "Ha Noi",
//                    ProfileImage       = "https://example.com/advisor2.jpg"
//                }
//            };

//            await context.Advisors.AddRangeAsync(advisors);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 3. INVESTORS
//        // =============================================
//        private static async Task SeedInvestorsAsync(ApplicationDbContext context)
//        {
//            if (await context.Investors.AnyAsync()) return;

//            var investorUsers = await context.Users
//                .Where(u => u.Role == UserRole.Investor)
//                .ToListAsync();

//            if (investorUsers.Count < 2)
//            {
//                Console.WriteLine("[Seeder] Không đủ Investor users, bỏ qua SeedInvestors");
//                return;
//            }

//            var investors = new List<Investor>
//            {
//                new Investor
//                {
//                    Id                  = Guid.NewGuid(),
//                    UserId              = investorUsers[0].Id,
//                    OrganizationName    = "VN Tech Ventures",
//                    InvestmentTaste     = "Early stage B2B SaaS, AI/ML startups",
//                    WalletAddress       = "0x1234567890abcdef",
//                    InvestmentAmount    = 500000m,
//                    InvestmentDate      = DateTime.UtcNow.AddMonths(-6),
//                    RiskTolerance       = RiskTolerance.High,
//                    InvestmentRegion    = "Southeast Asia",
//                    FocusIndustry       = "Technology, FinTech",
//                    PreferredStage      = PreferredStage.MVP,
//                    PreviousInvestments = "StartupX (exit 2x), TechY (active)"
//                },
//                new Investor
//                {
//                    Id                  = Guid.NewGuid(),
//                    UserId              = investorUsers[1].Id,
//                    OrganizationName    = "GreenGrowth Fund",
//                    InvestmentTaste     = "Sustainable agriculture, green energy",
//                    WalletAddress       = "0xabcdef1234567890",
//                    InvestmentAmount    = 300000m,
//                    InvestmentDate      = DateTime.UtcNow.AddMonths(-3),
//                    RiskTolerance       = RiskTolerance.Medium,
//                    InvestmentRegion    = "Vietnam, Cambodia",
//                    FocusIndustry       = "AgriTech, GreenTech",
//                    PreferredStage      = PreferredStage.Growth,
//                    PreviousInvestments = "FarmTech VN (active), EcoEnergy (exit 3x)"
//                }
//            };

//            await context.Investors.AddRangeAsync(investors);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 4. STARTUPS
//        // =============================================
//        private static async Task SeedStartupsAsync(ApplicationDbContext context)
//        {
//            if (await context.Startups.AnyAsync()) return;

//            var startupUsers = await context.Users
//                .Where(u => u.Role == UserRole.Startup)
//                .ToListAsync();

//            if (startupUsers.Count < 3)
//            {
//                Console.WriteLine("[Seeder] Không đủ Startup users, bỏ qua SeedStartups");
//                return;
//            }

//            var startups = new List<Startup>
//            {
//                new Startup
//                {
//                    Id                     = Guid.NewGuid(),
//                    UserId                 = startupUsers[0].Id,
//                    CompanyName            = "TechStart VN",
//                    Founder                = "Nguyen Van A",
//                    ContactInfo            = "techstart@gmail.com | 0901234567",
//                    CountryCity            = "Ho Chi Minh City, Vietnam",
//                    Website                = "https://techstart.vn",
//                    Industry               = "FinTech",
//                    DevelopmentStage       = DevelopmentStage.MVP,
//                    ProblemStatement       = "Khó khăn trong thanh toán số cho doanh nghiệp nhỏ",
//                    SolutionDescription    = "Nền tảng thanh toán số tích hợp AI",
//                    TargetCustomers        = "SMEs tại Việt Nam",
//                    UniqueValueProposition = "Phí thấp hơn 60% so với thị trường",
//                    MarketSize             = 5000000000m,
//                    BusinessModel          = "SaaS subscription + transaction fees",
//                    Revenue                = 50000m,
//                    TeamMembers            = "Nguyen Van A (CEO), Tran Thi B (CTO), Le Van C (CFO)",
//                    KeySkills              = "FinTech, Blockchain, Mobile Dev",
//                    TeamExperience         = "10+ years combined experience"
//                },
//                new Startup
//                {
//                    Id                     = Guid.NewGuid(),
//                    UserId                 = startupUsers[1].Id,
//                    CompanyName            = "GreenFarm Tech",
//                    Founder                = "Pham Thi B",
//                    ContactInfo            = "greenfarm@gmail.com | 0912345678",
//                    CountryCity            = "Can Tho, Vietnam",
//                    Website                = "https://greenfarm.tech",
//                    Industry               = "AgriTech",
//                    DevelopmentStage       = DevelopmentStage.Growth,
//                    ProblemStatement       = "Nông dân thiếu công cụ quản lý và bán hàng hiệu quả",
//                    SolutionDescription    = "App quản lý nông trại thông minh tích hợp IoT",
//                    TargetCustomers        = "Nông dân và hợp tác xã nông nghiệp",
//                    UniqueValueProposition = "Kết nối nông dân trực tiếp với người mua, tăng thu nhập 40%",
//                    MarketSize             = 2000000000m,
//                    BusinessModel          = "Commission + SaaS",
//                    Revenue                = 120000m,
//                    TeamMembers            = "Pham Thi B (CEO), Hoang Van D (CTO)",
//                    KeySkills              = "AgriTech, IoT, Mobile Dev",
//                    TeamExperience         = "8+ years in agriculture and technology"
//                },
//                new Startup
//                {
//                    Id                     = Guid.NewGuid(),
//                    UserId                 = startupUsers[2].Id,
//                    CompanyName            = "EduTech Solutions",
//                    Founder                = "Vo Van C",
//                    ContactInfo            = "edutech@gmail.com | 0923456789",
//                    CountryCity            = "Da Nang, Vietnam",
//                    Website                = "https://edutech.vn",
//                    Industry               = "EdTech",
//                    DevelopmentStage       = DevelopmentStage.Idea,
//                    ProblemStatement       = "Học sinh thiếu giáo viên giỏi tại vùng nông thôn",
//                    SolutionDescription    = "Nền tảng học trực tuyến kết nối giáo viên giỏi toàn quốc",
//                    TargetCustomers        = "Học sinh K-12 tại nông thôn",
//                    UniqueValueProposition = "Chi phí thấp, chất lượng cao nhờ AI personalization",
//                    MarketSize             = 3000000000m,
//                    BusinessModel          = "Freemium + Premium subscription",
//                    Revenue                = 0m,
//                    TeamMembers            = "Vo Van C (CEO), Nguyen Thi D (CTO)",
//                    KeySkills              = "EdTech, AI, UX Design",
//                    TeamExperience         = "5+ years in education and technology"
//                }
//            };

//            await context.Startups.AddRangeAsync(startups);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 5. PACKAGES
//        // =============================================
//        private static async Task SeedPackagesAsync(ApplicationDbContext context)
//        {
//            if (await context.Packages.AnyAsync()) return;

//            var packages = new List<Package>
//            {
//                new Package
//                {
//                    Id          = Guid.NewGuid(),
//                    PackageName = "Basic",
//                    Description = "Gói cơ bản - Phù hợp cho startup mới",
//                    Price       = 99000m,
//                    Duration    = 30
//                },
//                new Package
//                {
//                    Id          = Guid.NewGuid(),
//                    PackageName = "Pro",
//                    Description = "Gói Pro - Đầy đủ tính năng cho startup tăng trưởng",
//                    Price       = 299000m,
//                    Duration    = 30
//                },
//                new Package
//                {
//                    Id          = Guid.NewGuid(),
//                    PackageName = "Enterprise",
//                    Description = "Gói doanh nghiệp - Cho startup giai đoạn scale",
//                    Price       = 999000m,
//                    Duration    = 30
//                }
//            };

//            await context.Packages.AddRangeAsync(packages);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 6. WALLETS
//        // =============================================
//        private static async Task SeedWalletsAsync(ApplicationDbContext context)
//        {
//            if (await context.Wallets.AnyAsync()) return;

//            var users = await context.Users.ToListAsync();

//            var wallets = users.Select(u => new Wallet
//            {
//                Id       = Guid.NewGuid(),
//                UserId   = u.Id,
//                Balance  = u.Role == UserRole.Investor ? 10000000m
//                         : u.Role == UserRole.Advisor   ? 5000000m
//                         : 1000000m,
//                Currency = "VND",
//                IsActive = true
//            }).ToList();

//            await context.Wallets.AddRangeAsync(wallets);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 7. BOOKINGS
//        // =============================================
//        private static async Task SeedBookingsAsync(ApplicationDbContext context)
//        {
//            if (await context.Bookings.AnyAsync()) return;

//            var advisor   = await context.Advisors.FirstOrDefaultAsync();
//            var customers = await context.Users
//                .Where(u => u.Role == UserRole.Startup)
//                .ToListAsync();

//            if (advisor == null || !customers.Any()) return;

//            var bookings = new List<Booking>
//            {
//                new Booking
//                {
//                    Id         = Guid.NewGuid(),
//                    AdvisorId  = advisor.Id,
//                    CustomerId = customers[0].Id,
//                    StartTime  = DateTime.UtcNow.AddDays(1),
//                    EndTime    = DateTime.UtcNow.AddDays(1).AddHours(1),
//                    Price      = 500000m,
//                    Status     = BookingStatus.Confirmed
//                },
//                new Booking
//                {
//                    Id         = Guid.NewGuid(),
//                    AdvisorId  = advisor.Id,
//                    CustomerId = customers[1].Id,
//                    StartTime  = DateTime.UtcNow.AddDays(3),
//                    EndTime    = DateTime.UtcNow.AddDays(3).AddHours(1),
//                    Price      = 500000m,
//                    Status     = BookingStatus.Pending
//                },
//                new Booking
//                {
//                    Id         = Guid.NewGuid(),
//                    AdvisorId  = advisor.Id,
//                    CustomerId = customers[0].Id,
//                    StartTime  = DateTime.UtcNow.AddDays(-5),
//                    EndTime    = DateTime.UtcNow.AddDays(-5).AddHours(1),
//                    Price      = 500000m,
//                    Status     = BookingStatus.Completed
//                }
//            };

//            await context.Bookings.AddRangeAsync(bookings);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 8. PROJECTS
//        // =============================================
//        private static async Task SeedProjectsAsync(ApplicationDbContext context)
//        {
//            if (await context.Projects.AnyAsync()) return;

//            var startupUsers = await context.Users
//                .Where(u => u.Role == UserRole.Startup)
//                .ToListAsync();

//            if (startupUsers.Count < 3)
//            {
//                Console.WriteLine("[Seeder] Không đủ Startup users, bỏ qua SeedProjects");
//                return;
//            }

//            var projects = new List<Project>
//            {
//                new Project
//                {
//                    Id              = Guid.NewGuid(),
//                    UserId          = startupUsers[0].Id,
//                    ProjectName     = "AISEP Payment Module",
//                    Description     = "Module thanh toán thông minh cho SMEs",
//                    FullDescription = "Xây dựng hệ thống thanh toán tích hợp AI để tối ưu hóa dòng tiền cho doanh nghiệp vừa và nhỏ tại Việt Nam",
//                    Status          = ProjectStatus.InProgress
//                },
//                new Project
//                {
//                    Id              = Guid.NewGuid(),
//                    UserId          = startupUsers[1].Id,
//                    ProjectName     = "Smart Farm IoT",
//                    Description     = "Hệ thống IoT quản lý nông trại thông minh",
//                    FullDescription = "Triển khai cảm biến IoT và AI để theo dõi, phân tích và tối ưu hóa năng suất nông nghiệp",
//                    Status          = ProjectStatus.InProgress
//                },
//                new Project
//                {
//                    Id              = Guid.NewGuid(),
//                    UserId          = startupUsers[2].Id,
//                    ProjectName     = "EduConnect Platform",
//                    Description     = "Nền tảng kết nối giáo viên và học sinh",
//                    FullDescription = "Phát triển ứng dụng mobile kết nối giáo viên giỏi với học sinh ở vùng nông thôn",
//                    Status          = ProjectStatus.Draft
//                }
//            };

//            await context.Projects.AddRangeAsync(projects);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 9. CONNECTION REQUESTS
//        // =============================================
//        private static async Task SeedConnectionRequestsAsync(ApplicationDbContext context)
//        {
//            if (await context.ConnectionRequests.AnyAsync()) return;

//            var investor = await context.Investors.FirstOrDefaultAsync();
//            var startups = await context.Startups.ToListAsync();

//            if (investor == null || !startups.Any()) return;

//            var requests = new List<ConnectionRequest>
//            {
//                new ConnectionRequest
//                {
//                    Id           = Guid.NewGuid(),
//                    InvestorId   = investor.Id,
//                    StartupId    = startups[0].Id,
//                    Status       = ConnectionRequestStatus.Accepted,
//                    RequestDate  = DateTime.UtcNow.AddDays(-10),
//                    ResponseDate = DateTime.UtcNow.AddDays(-8),
//                    Message      = "Chúng tôi rất quan tâm đến giải pháp thanh toán của các bạn",
//                    Reason       = null
//                },
//                new ConnectionRequest
//                {
//                    Id           = Guid.NewGuid(),
//                    InvestorId   = investor.Id,
//                    StartupId    = startups[1].Id,
//                    Status       = ConnectionRequestStatus.Pending,
//                    RequestDate  = DateTime.UtcNow.AddDays(-2),
//                    ResponseDate = null,
//                    Message      = "AgriTech là lĩnh vực chúng tôi đang tập trung đầu tư",
//                    Reason       = null
//                }
//            };

//            await context.ConnectionRequests.AddRangeAsync(requests);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 10. REVIEWS
//        // =============================================
//        private static async Task SeedReviewsAsync(ApplicationDbContext context)
//        {
//            if (await context.Reviews.AnyAsync()) return;

//            var advisor       = await context.Advisors.FirstOrDefaultAsync();
//            var reviewerUsers = await context.Users
//                .Where(u => u.Role == UserRole.Startup)
//                .ToListAsync();

//            if (advisor == null || !reviewerUsers.Any()) return;

//            var reviews = new List<Review>
//            {
//                new Review
//                {
//                    Id            = Guid.NewGuid(),
//                    AdvisorId     = advisor.Id,
//                    ReviewerId    = reviewerUsers[0].Id,
//                    Rating        = 5,
//                    ReviewContent = "Advisor rất chuyên nghiệp, giúp chúng tôi định hình được chiến lược kinh doanh rõ ràng",
//                    CreatedAt     = DateTime.UtcNow.AddDays(-3)
//                },
//                new Review
//                {
//                    Id            = Guid.NewGuid(),
//                    AdvisorId     = advisor.Id,
//                    ReviewerId    = reviewerUsers[1].Id,
//                    Rating        = 4,
//                    ReviewContent = "Tư vấn rất tốt về mô hình kinh doanh và chiến lược go-to-market",
//                    CreatedAt     = DateTime.UtcNow.AddDays(-1)
//                }
//            };

//            await context.Reviews.AddRangeAsync(reviews);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 11. NOTIFICATIONS
//        // =============================================
//        private static async Task SeedNotificationsAsync(ApplicationDbContext context)
//        {
//            if (await context.Notifications.AnyAsync()) return;

//            var users = await context.Users.Take(5).ToListAsync();

//            var notifications = users.SelectMany(u => new[]
//            {
//                new Notification
//                {
//                    Id        = Guid.NewGuid(),
//                    UserId    = u.Id,
//                    Message   = "Chào mừng bạn đến với AISEP Platform!",
//                    Status    = NotificationStatus.Unread,
//                    CreatedAt = DateTime.UtcNow
//                },
//                new Notification
//                {
//                    Id        = Guid.NewGuid(),
//                    UserId    = u.Id,
//                    Message   = "Hồ sơ của bạn đã được xác minh thành công",
//                    Status    = NotificationStatus.Read,
//                    CreatedAt = DateTime.UtcNow.AddHours(-2)
//                }
//            }).ToList();

//            await context.Notifications.AddRangeAsync(notifications);
//            await context.SaveChangesAsync();
//        }

//        // =============================================
//        // 12. SUBSCRIPTIONS
//        // =============================================
//        private static async Task SeedSubscriptionsAsync(ApplicationDbContext context)
//        {
//            if (await context.Subscriptions.AnyAsync()) return;

//            var package      = await context.Packages.FirstOrDefaultAsync(p => p.PackageName == "Pro");
//            var startupUsers = await context.Users
//                .Where(u => u.Role == UserRole.Startup)
//                .ToListAsync();

//            if (package == null || !startupUsers.Any()) return;

//            var subscriptions = startupUsers.Take(2).Select(u => new Subscription
//            {
//                Id        = Guid.NewGuid(),
//                PackageId = package.Id,
//                UserId    = u.Id,
//                StartDate = DateTime.UtcNow,
//                EndDate   = DateTime.UtcNow.AddDays(30),
//                Status    = SubscriptionStatus.Active
//            }).ToList();

//            await context.Subscriptions.AddRangeAsync(subscriptions);
//            await context.SaveChangesAsync();
//        }
//    }
//}
