using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs.Requests
{
    public class CreateReviewRequest
    {
        [Required]
        public int AdvisorId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(2000)]
        public string? ReviewContent { get; set; }
    }
}
