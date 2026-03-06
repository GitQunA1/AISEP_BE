using AISEP.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AISEP.DTOs
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Project name is required.")]
        [MinLength(3, ErrorMessage = "Project name must be at least 3 characters.")]
        [MaxLength(255, ErrorMessage = "Project name cannot exceed 255 characters.")]
        public string ProjectName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Short description cannot exceed 500 characters.")]
        public string? ShortDescription { get; set; }

        public DevelopmentStage? DevelopmentStage { get; set; }

        [MaxLength(2000, ErrorMessage = "Problem statement cannot exceed 2000 characters.")]
        public string? ProblemStatement { get; set; }

        [MaxLength(2000, ErrorMessage = "Solution description cannot exceed 2000 characters.")]
        public string? SolutionDescription { get; set; }

        [MaxLength(1000, ErrorMessage = "Target customers cannot exceed 1000 characters.")]
        public string? TargetCustomers { get; set; }

        [MaxLength(1000, ErrorMessage = "Unique value proposition cannot exceed 1000 characters.")]
        public string? UniqueValueProposition { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Market size must be a non-negative number.")]
        public decimal? MarketSize { get; set; }

        [MaxLength(1000, ErrorMessage = "Business model cannot exceed 1000 characters.")]
        public string? BusinessModel { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Revenue must be a non-negative number.")]
        public decimal? Revenue { get; set; }

        [MaxLength(1000, ErrorMessage = "Competitors cannot exceed 1000 characters.")]
        public string? Competitors { get; set; }

        [MaxLength(1000, ErrorMessage = "Team members cannot exceed 1000 characters.")]
        public string? TeamMembers { get; set; }

        [MaxLength(500, ErrorMessage = "Key skills cannot exceed 500 characters.")]
        public string? KeySkills { get; set; }

        [MaxLength(1000, ErrorMessage = "Team experience cannot exceed 1000 characters.")]
        public string? TeamExperience { get; set; }
    }
}
