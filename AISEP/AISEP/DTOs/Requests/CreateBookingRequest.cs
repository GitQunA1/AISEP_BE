using AISEP.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs.Requests
{
    public class CreateBookingRequest
    {
        [Required(ErrorMessage = "AdvisorId is required")]
        public int AdvisorId { get; set; }

        [Required(ErrorMessage = "StartTime is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "EndTime is required")]
        public DateTime EndTime { get; set; }
    }
}
