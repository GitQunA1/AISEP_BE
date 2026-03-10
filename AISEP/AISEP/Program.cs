using AISEP.Common;
using AISEP.Data;
using AISEP.Models.Entities;
using AISEP.Repositories.Investors;
using AISEP.Repositories.Projects;
using AISEP.Repositories.Startups;
using AISEP.Models;
using AISEP.Services.Auth;
using AISEP.Services.Blockchain;
using AISEP.Services.Bookings;
using AISEP.Services.Users;
using AISEP.Services.Documents;
using AISEP.Services.Email;
using AISEP.Services.Investors;
using AISEP.Services.Jwt;
using AISEP.Services.Projects;
using AISEP.Services.Reviews;
using AISEP.Services.Startups;
using AISEP.Services.StartupFollowers;
using AISEP.Services.Storage;
using AISEP.Services.AI;
using AISEP.Settings;
using AISEP.Validators.Auth;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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
builder.Services.AddHttpClient<IGeminiAiService, GeminiAiService>();
builder.Services.AddScoped<IStartupAIAnalysisService, StartupAIAnalysisService>();

// Add DbContext
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
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IStartupFollowerService, StartupFollowerService>();
builder.Services.AddScoped<IStartupService, StartupService>();
builder.Services.AddScoped<IInvestorService, InvestorService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IStorageService, CloudinaryStorageService>();
builder.Services.AddScoped<IBlockchainService, SepoliaBlockchainService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();


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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed data mẫu
await DataSeeder.SeedAsync(app.Services);

app.Run();


// Test CI/CD on Program.cs