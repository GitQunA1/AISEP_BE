using AISEP.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace AISEP.BLL.DTOs.Requests
{
    public class CreateStartupRequest
    {
        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? LogoUrl { get; set; }

        [MaxLength(255)]
        public string? Founder { get; set; }

        public string? ContactInfo { get; set; }

        [MaxLength(255)]
        public string? CountryCity { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        public Industry? Industry { get; set; }

        [MaxLength(255)]
        public string? BusinessLicenseUrl { get; set; }
    }
}
