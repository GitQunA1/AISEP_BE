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


            // Advisor
            mapper.Property<Advisor>(a => a.Rating)
                .CanFilter()
                .CanSort();
            // User
            mapper.Property<User>(u => u.Email)
                .CanFilter()
                .CanSort();
            //Reviews
            mapper.Property<Review>(r => r.Rating)
                .CanFilter()
                .CanSort();
            mapper.Property<Review>(r => r.CreatedAt)
                .CanFilter()
                .CanSort();
            //StartupFollower
            mapper.Property<StartupFollower>(sf => sf.CreatedAt)
                .CanFilter()
                .CanSort();

            // Startup
            mapper.Property<Startup>(s => s.CompanyName)
                .CanFilter()
                .CanSort();
            mapper.Property<Startup>(s => s.Industry)
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
            mapper.Property<Investor>(i => i.FocusIndustry)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.InvestmentRegion)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.RiskTolerance)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.PreferredStage)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.InvestmentAmount)
                .CanFilter()
                .CanSort();
            mapper.Property<Investor>(i => i.InvestmentDate)
                .CanFilter()
                .CanSort();

            // Project
            mapper.Property<Project>(p => p.ProjectName)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.Status)
                .CanFilter()
                .CanSort();
            mapper.Property<Project>(p => p.DevelopmentStage)
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

            return mapper;
        }
    }
}
