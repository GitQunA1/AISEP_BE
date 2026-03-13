using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace AISEP.DAL.Data
{
    // Used by EF Core tools (dotnet ef) at design time to create the DbContext
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try to read connection string from API project's appsettings.json first,
            // then from environment variable DefaultConnection, otherwise fall back to a local default.
            var basePath = Directory.GetCurrentDirectory();

            // API project is located at ../AISEP.API relative to DAL project
            var apiSettingsPath = Path.Combine(basePath, "..", "AISEP.API", "appsettings.json");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(apiSettingsPath, optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? Environment.GetEnvironmentVariable("DefaultConnection")
                                   ?? "Host=localhost;Database=aisep_dev;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
