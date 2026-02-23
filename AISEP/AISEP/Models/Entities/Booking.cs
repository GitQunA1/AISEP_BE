using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AISEP.Models.Enums;

namespace AISEP.Models.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }

        public Guid AdvisorId { get; set; }

        public Guid CustomerId { get; set; }

        public DateTime StartTime { get; set; }

        public decimal Price { get; set; }

        public DateTime EndTime { get; set; }

        public BookingStatus Status { get; set; }

        // Navigation properties
        public Advisor Advisor { get; set; } = null!;

        public User Customer { get; set; } = null!;

        public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
        public ICollection<ConsultingReport> ConsultingReports { get; set; } = new List<ConsultingReport>();
    }
}
