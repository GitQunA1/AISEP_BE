using AISEP.DAL.Enums;

namespace AISEP.BLL.DTOs.Responses
{
    public class AdvisorResponse
    {
        public int    AdvisorId          { get; set; }
        public int    UserId             { get; set; }
        public string? UserName          { get; set; }
        public string? Email             { get; set; }
        public string? Bio               { get; set; }
        public string? Expertise         { get; set; }
        public string? Certifications    { get; set; }
        public string? PreviousExperience { get; set; }
        public decimal? Rating           { get; set; }
        public string? LanguagesSpoken   { get; set; }
        public string? Location          { get; set; }
        public string? ProfileImage      { get; set; }
        public string? Industry          { get; set; }
        public decimal? HourlyRate       { get; set; }
        public string? ApprovalStatus    { get; set; }
    }
}
