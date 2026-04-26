using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Requests
{
    public class CreateAdvisorRequest
    {
        public string?   Bio                { get; set; }
        public string?   Expertise          { get; set; }
        public List<int>? IndustryOptionIds { get; set; }
        public string?   PreviousExperience { get; set; }
        public string?   LanguagesSpoken    { get; set; }
        public string?   Location           { get; set; }
        public decimal? HourlyRate         { get; set; }
        public IFormFile? ProfileImageFile  { get; set; }
        public IFormFile? CertificationFile { get; set; }
    }
}
