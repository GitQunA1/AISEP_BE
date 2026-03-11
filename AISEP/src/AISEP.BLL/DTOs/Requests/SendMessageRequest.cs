using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class SendMessageRequest
    {
        [Required]
        [MinLength(1)]
        public string Content { get; set; } = string.Empty;
    }
}
