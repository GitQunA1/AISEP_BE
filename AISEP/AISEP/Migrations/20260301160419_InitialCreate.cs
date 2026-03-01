using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "packages",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_packages", x => x.PackageId);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_role_claims_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "actionlogs",
                columns: table => new
                {
                    ActionLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actionlogs", x => x.ActionLogId);
                    table.ForeignKey(
                        name: "FK_actionlogs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "advisors",
                columns: table => new
                {
                    AdvisorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    Expertise = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Certifications = table.Column<string>(type: "text", nullable: true),
                    PreviousExperience = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    LanguagesSpoken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProfileImage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advisors", x => x.AdvisorId);
                    table.ForeignKey(
                        name: "FK_advisors_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "investors",
                columns: table => new
                {
                    InvestorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    InvestmentTaste = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    WalletAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    InvestmentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InvestmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RiskTolerance = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InvestmentRegion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FocusIndustry = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PreferredStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PreviousInvestments = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investors", x => x.InvestorId);
                    table.ForeignKey(
                        name: "FK_investors_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    FullDescription = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_projects_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    RefreshTokenId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedByIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "startups",
                columns: table => new
                {
                    StartupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Founder = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContactInfo = table.Column<string>(type: "text", nullable: true),
                    CountryCity = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Industry = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DevelopmentStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProblemStatement = table.Column<string>(type: "text", nullable: true),
                    SolutionDescription = table.Column<string>(type: "text", nullable: true),
                    TargetCustomers = table.Column<string>(type: "text", nullable: true),
                    UniqueValueProposition = table.Column<string>(type: "text", nullable: true),
                    MarketSize = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BusinessModel = table.Column<string>(type: "text", nullable: true),
                    Revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Competitors = table.Column<string>(type: "text", nullable: true),
                    TeamMembers = table.Column<string>(type: "text", nullable: true),
                    KeySkills = table.Column<string>(type: "text", nullable: true),
                    TeamExperience = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_startups", x => x.StartupId);
                    table.ForeignKey(
                        name: "FK_startups_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_subscriptions_packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_transactions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_claims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_user_logins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReporterId = table.Column<int>(type: "integer", nullable: false),
                    ReportedUserId = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    EvidenceUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_user_reports_users_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_reports_users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_user_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    WalletId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => x.WalletId);
                    table.ForeignKey(
                        name: "FK_wallets_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvisorId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_bookings_advisors_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "advisors",
                        principalColumn: "AdvisorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bookings_users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvisorId = table.Column<int>(type: "integer", nullable: false),
                    ReviewerId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    ReviewContent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_reviews_advisors_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "advisors",
                        principalColumn: "AdvisorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "deals",
                columns: table => new
                {
                    DealId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvestorId = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StartupConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    InvestorConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DealDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EquityPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    TransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deals", x => x.DealId);
                    table.ForeignKey(
                        name: "FK_deals_investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "investors",
                        principalColumn: "InvestorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_deals_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BlockchainTxHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsIpProtected = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_documents_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "investor_ai_analyses",
                columns: table => new
                {
                    AnalysisId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvestorId = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    AnalysisJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investor_ai_analyses", x => x.AnalysisId);
                    table.ForeignKey(
                        name: "FK_investor_ai_analyses_investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "investors",
                        principalColumn: "InvestorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_investor_ai_analyses_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "startup_ai_analyses",
                columns: table => new
                {
                    EvaluationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    PotentialScore = table.Column<int>(type: "integer", nullable: true),
                    ChaosScore = table.Column<int>(type: "integer", nullable: true),
                    AnalysisJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_startup_ai_analyses", x => x.EvaluationId);
                    table.ForeignKey(
                        name: "FK_startup_ai_analyses_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connectionrequests",
                columns: table => new
                {
                    ConnectionRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvestorId = table.Column<int>(type: "integer", nullable: false),
                    StartupId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponseMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connectionrequests", x => x.ConnectionRequestId);
                    table.ForeignKey(
                        name: "FK_connectionrequests_investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "investors",
                        principalColumn: "InvestorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_connectionrequests_startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "startups",
                        principalColumn: "StartupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "startup_followers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    StartupId = table.Column<int>(type: "integer", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_startup_followers", x => new { x.UserId, x.StartupId });
                    table.ForeignKey(
                        name: "FK_startup_followers_startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "startups",
                        principalColumn: "StartupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_startup_followers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallettransactions",
                columns: table => new
                {
                    WalletTransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallettransactions", x => x.WalletTransactionId);
                    table.ForeignKey(
                        name: "FK_wallettransactions_wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "withdrawrequests",
                columns: table => new
                {
                    WithdrawRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdrawrequests", x => x.WithdrawRequestId);
                    table.ForeignKey(
                        name: "FK_withdrawrequests_wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chatsessions",
                columns: table => new
                {
                    ChatSessionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatsessions", x => x.ChatSessionId);
                    table.ForeignKey(
                        name: "FK_chatsessions_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consultingreports",
                columns: table => new
                {
                    ConsultingReportId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    MeetingTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MeetingTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MeetingPurpose = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    DecisionsMade = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultingreports", x => x.ConsultingReportId);
                    table.ForeignKey(
                        name: "FK_consultingreports_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nftrecords",
                columns: table => new
                {
                    NFTRecordId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DealId = table.Column<int>(type: "integer", nullable: false),
                    TokenId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TxHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OwnerWallet = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ValidityStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Transferable = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousOwnerWallet = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nftrecords", x => x.NFTRecordId);
                    table.ForeignKey(
                        name: "FK_nftrecords_deals_DealId",
                        column: x => x.DealId,
                        principalTable: "deals",
                        principalColumn: "DealId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blockchainproof",
                columns: table => new
                {
                    BlockchainProofId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    TransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    VerificationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchainproof", x => x.BlockchainProofId);
                    table.ForeignKey(
                        name: "FK_blockchainproof_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConnectionRequestId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    StartupApproved = table.Column<bool>(type: "boolean", nullable: false),
                    InvestorApproved = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_posts_connectionrequests_ConnectionRequestId",
                        column: x => x.ConnectionRequestId,
                        principalTable: "connectionrequests",
                        principalColumn: "ConnectionRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chatmessages",
                columns: table => new
                {
                    ChatMessageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatSessionId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chatmessages", x => x.ChatMessageId);
                    table.ForeignKey(
                        name: "FK_chatmessages_chatsessions_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalTable: "chatsessions",
                        principalColumn: "ChatSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chatmessages_users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_actionlogs_UserId",
                table: "actionlogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_advisors_UserId",
                table: "advisors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blockchainproof_DocumentId",
                table: "blockchainproof",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_AdvisorId",
                table: "bookings",
                column: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_CustomerId",
                table: "bookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_chatmessages_ChatSessionId",
                table: "chatmessages",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_chatmessages_SenderId",
                table: "chatmessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_chatsessions_BookingId",
                table: "chatsessions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_connectionrequests_InvestorId",
                table: "connectionrequests",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_connectionrequests_StartupId",
                table: "connectionrequests",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_consultingreports_BookingId",
                table: "consultingreports",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_deals_InvestorId",
                table: "deals",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_deals_ProjectId",
                table: "deals",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_ProjectId",
                table: "documents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_investor_ai_analyses_InvestorId",
                table: "investor_ai_analyses",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_investor_ai_analyses_ProjectId",
                table: "investor_ai_analyses",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_investors_UserId",
                table: "investors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nftrecords_DealId",
                table: "nftrecords",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId",
                table: "notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_posts_ConnectionRequestId",
                table: "posts",
                column: "ConnectionRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_UserId",
                table: "projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_AdvisorId",
                table: "reviews",
                column: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_ReviewerId",
                table: "reviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_role_claims_RoleId",
                table: "role_claims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_startup_ai_analyses_ProjectId",
                table: "startup_ai_analyses",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_startup_followers_StartupId",
                table: "startup_followers",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_startups_UserId",
                table: "startups",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_PackageId",
                table: "subscriptions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId",
                table: "subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_UserId",
                table: "transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_claims_UserId",
                table: "user_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_logins_UserId",
                table: "user_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ReportedUserId",
                table: "user_reports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ReporterId",
                table: "user_reports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallets_UserId",
                table: "wallets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_wallettransactions_WalletId",
                table: "wallettransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_withdrawrequests_WalletId",
                table: "withdrawrequests",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "actionlogs");

            migrationBuilder.DropTable(
                name: "blockchainproof");

            migrationBuilder.DropTable(
                name: "chatmessages");

            migrationBuilder.DropTable(
                name: "consultingreports");

            migrationBuilder.DropTable(
                name: "investor_ai_analyses");

            migrationBuilder.DropTable(
                name: "nftrecords");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "startup_ai_analyses");

            migrationBuilder.DropTable(
                name: "startup_followers");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "user_reports");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "wallettransactions");

            migrationBuilder.DropTable(
                name: "withdrawrequests");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "chatsessions");

            migrationBuilder.DropTable(
                name: "deals");

            migrationBuilder.DropTable(
                name: "connectionrequests");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "wallets");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "investors");

            migrationBuilder.DropTable(
                name: "startups");

            migrationBuilder.DropTable(
                name: "advisors");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
