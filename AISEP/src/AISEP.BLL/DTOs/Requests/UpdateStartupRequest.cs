using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class UpdateStartupRequest
    {
        public string?    CompanyName         { get; set; }
        public string?    Founder             { get; set; }
        public string?    Email               { get; set; }
        public string?    PhoneNumber         { get; set; }
        public string?    CountryCity         { get; set; }
        public string?    Website             { get; set; }
        public List<int>? IndustryOptionIds   { get; set; }
        public IFormFile? LogoFile            { get; set; }
        public IFormFile? BusinessLicenseFile { get; set; }
    }
}
