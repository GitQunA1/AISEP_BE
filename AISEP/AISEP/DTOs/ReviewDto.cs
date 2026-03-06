using System.ComponentModel.DataAnnotations;

<<<<<<<< HEAD:AISEP/AISEP/DTOs/ReviewDto.cs
namespace AISEP.DTOs
========
namespace AISEP.DTOs.Requests
>>>>>>>> NHQuan:AISEP/AISEP/DTOs/Requests/CreateReviewRequest.cs
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
