using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Price { get; set; }
        public BookingStatus Status { get; set; }
    }
}
