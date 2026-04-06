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
            //ProjectFollower
            mapper.Property<ProjectFollower>(pf => pf.CreatedAt)
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
            mapper.Property<Project>(p => p.Industry)
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

            return mapper;
        }
    }
}
