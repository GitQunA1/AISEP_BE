namespace AISEP.BLL.DTOs.Requests
{
    public class AdvisorRequest
    {
        public string? Bio                { get; set; }
        public string? Expertise          { get; set; }
        public string? Certifications     { get; set; }
        public string? PreviousExperience { get; set; }
        public string? LanguagesSpoken    { get; set; }
        public string? Location           { get; set; }
        public string? ProfileImage       { get; set; }
        public decimal? HourlyRate        { get; set; }
    }
}
