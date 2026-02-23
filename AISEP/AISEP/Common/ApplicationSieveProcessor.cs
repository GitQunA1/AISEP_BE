using AISEP.Models.Entities;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Common
{
    public class ApplicationSieveProcessor : SieveProcessor
    {
        public ApplicationSieveProcessor(IOptions<SieveOptions> options) : base(options)
        {

        }
        /// <summary>
        /// Map properties của các models để có thể filter/sort
        /// </summary>
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
            return mapper;
        }
    }
}
