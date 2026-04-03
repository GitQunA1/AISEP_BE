using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Data;
using AISEP.DAL.Entities;
using AISEP.BLL.Services.Auth;
using AISEP.BLL.Services.Blockchain;
using AISEP.BLL.Services.Bookings;
using AISEP.BLL.Services.Users;
using AISEP.BLL.Services.UserReports;
using AISEP.BLL.Services.Documents;
using AISEP.BLL.Services.ConsultingReports;
using AISEP.BLL.Services.Email;
using AISEP.BLL.Services.Investors;
using AISEP.BLL.Services.Jwt;
using AISEP.BLL.Services.Projects;
using AISEP.BLL.Services.Reviews;
using AISEP.BLL.Services.Startups;
using AISEP.BLL.Services.ProjectFollowers;
using AISEP.BLL.Services.ProjectAdvisorAssignments;
using AISEP.BLL.Services.Storage;
using AISEP.BLL.Services.Advisors;
using AISEP.BLL.Services.AdvisorAvailabilities;
using AISEP.BLL.Services.AI;
using AISEP.BLL.Services.Chats;
using AISEP.BLL.Services.Payments;
using AISEP.BLL.Services.Notifications;
using AISEP.BLL.Services.Connections;
using AISEP.BLL.Services.Deals;
using AISEP.BLL.Services.Pinata;
using AISEP.BLL.Services.BackgroundServices;
using AISEP.BLL.Settings;
using AISEP.BLL.Validators.Auth;
using AISEP.API.Middleware;
using AISEP.API.Hubs;
using AISEP.API.Realtime;
//using AISEP.API.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Sieve.Models;
using Sieve.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://aisep.tech",
                "https://www.aisep.tech")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Add HttpContextAccessor (required for CurrentUserService)
builder.Services.AddHttpContextAccessor();

// Configure JwtSettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Configure EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure BlockchainSettings
builder.Services.Configure<BlockchainSettings>(builder.Configuration.GetSection("BlockchainSettings"));

// Configure CloudinarySettings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Configure GeminiSettings
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));

// Configure SePaySettings
builder.Services.Configure<SePaySettings>(builder.Configuration.GetSection("SePaySettings"));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IGeminiAiService, GeminiAiService>();
builder.Services.AddScoped<IStartupAIAnalysisService, StartupAIAnalysisService>();
builder.Services.AddScoped<IInvestorAIAnalysisService, InvestorAIAnalysisService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Add Sieve
builder.Services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
builder.Services.Configure<SieveOptions>(builder.Configuration.GetSection("Sieve"));
// Add Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
  // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

 // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    // Let FluentValidation handle Vietnamese + spaces username format.
    options.User.AllowedUserNameCharacters = string.Empty;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
   options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "")),
  ClockSkew = TimeSpan.Zero
   };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var requestPath = context.HttpContext.Request.Path;

            if (!string.IsNullOrWhiteSpace(accessToken)
                && (requestPath.StartsWithSegments("/hubs/notifications")
                    || requestPath.StartsWithSegments("/hubs/chat")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.ErrorResponse(
                "Unauthorized access. Please provide a valid token JWT.",
                "Unauthorized",
                401);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        },
        OnForbidden = async context =>
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.ErrorResponse(
                "You do not have permission to access this resource.",
                "Forbidden",
                403);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    };
});

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Sieve for filtering, sorting, and paging
builder.Services.AddScoped<Sieve.Services.ISieveProcessor, ApplicationSieveProcessor>();
builder.Services.Configure<Sieve.Models.SieveOptions>(builder.Configuration.GetSection("Sieve"));

//// Add Repositories
//builder.Services.AddScoped<IStartupRepository, StartupRepository>();
//builder.Services.AddScoped<IInvestorRepository, InvestorRepository>();

// Add Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserReportService, UserReportService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IProjectFollowerService, ProjectFollowerService>();
builder.Services.AddScoped<IProjectAdvisorAssignmentService, ProjectAdvisorAssignmentService>();
builder.Services.AddScoped<IStartupService, StartupService>();
builder.Services.AddScoped<IInvestorService, InvestorService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IStorageService, CloudinaryStorageService>();
builder.Services.AddScoped<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IAdvisorService, AdvisorService>();
builder.Services.AddScoped<IAdvisorAvailabilityService, AdvisorAvailabilityService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IConsultingReportService, ConsultingReportService>();
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IConnectionService, ConnectionService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IPinataService, PinataService>();
builder.Services.AddScoped<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();
builder.Services.AddHostedService<SubscriptionExpiryBackgroundService>();
builder.Services.AddHostedService<BookingResponseExpiryBackgroundService>();
builder.Services.AddHostedService<ConsultingReportDeadlineBackgroundService>();


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage);
        return new BadRequestObjectResult(ApiResponse<object>.ErrorResponse(errors.ToList(), string.Join(" | ", errors)));
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.DescribeAllParametersInCamelCase();
    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    []
                }
            });
});

var app = builder.Build();

//await DatabaseSeeder.SeedAsync(app);

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
