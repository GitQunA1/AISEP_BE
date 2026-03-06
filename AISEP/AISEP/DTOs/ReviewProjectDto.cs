using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs
{
    public class ReviewProjectDto
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }
}
