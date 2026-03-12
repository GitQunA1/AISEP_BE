using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateStartupRequest
    {
        public string?    CompanyName         { get; set; }
        public string?    Founder             { get; set; }
        public string?    ContactInfo         { get; set; }
        public string?    CountryCity         { get; set; }
        public string?    Website             { get; set; }
        public Industry?  Industry            { get; set; }
        public IFormFile? LogoFile            { get; set; }
        public IFormFile? BusinessLicenseFile { get; set; }
    }
}
