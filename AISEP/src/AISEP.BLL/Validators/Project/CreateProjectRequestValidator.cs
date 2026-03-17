using AISEP.BLL.DTOs.Requests;
using AISEP.DAL.Enums;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        private const string TextPattern =
            @"^[\p{L}\p{N}\s.,!?'-]*$";

        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.DevelopmentStage)
                .IsInEnum().WithMessage("Development stage is invalid.");

            // IDEA: required for all stages (Idea, MVP, Growth)
            RuleFor(x => x.ProjectName)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(255).WithMessage("Project name must not exceed 255 characters.")
                .Matches(TextPattern).WithMessage("Project name contains invalid characters.");

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description is required.")
                .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.")
                .Matches(TextPattern).WithMessage("Short description contains invalid characters.");

            RuleFor(x => x.ProblemStatement)
                .NotEmpty().WithMessage("Problem statement is required.")
                .MaximumLength(2000).WithMessage("Problem statement must not exceed 2000 characters.")
                .Matches(TextPattern).WithMessage("Problem statement contains invalid characters.");

            RuleFor(x => x.SolutionDescription)
                .NotEmpty().WithMessage("Solution description is required.")
                .MaximumLength(2000).WithMessage("Solution description must not exceed 2000 characters.")
                .Matches(TextPattern).WithMessage("Solution description contains invalid characters.");

            RuleFor(x => x.TargetCustomers)
                .NotEmpty().WithMessage("Target customers is required.")
                .MaximumLength(1000).WithMessage("Target customers must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Target customers contains invalid characters.");

            RuleFor(x => x.TeamMembers)
                .NotEmpty().WithMessage("Team members is required.")
                .MaximumLength(1000).WithMessage("Team members must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Team members contains invalid characters.");

            // Optional text fields: validate format only when provided
            RuleFor(x => x.UniqueValueProposition)
                .MaximumLength(1000).WithMessage("Unique value proposition must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Unique value proposition contains invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.UniqueValueProposition));

            RuleFor(x => x.BusinessModel)
                .MaximumLength(1000).WithMessage("Business model must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Business model contains invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.BusinessModel));

            RuleFor(x => x.KeySkills)
                .MaximumLength(1000).WithMessage("Key skills must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Key skills contain invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.KeySkills));

            RuleFor(x => x.Competitors)
                .MaximumLength(1000).WithMessage("Competitors must not exceed 1000 characters.")
                .Matches(TextPattern).WithMessage("Competitors contain invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Competitors));

            RuleFor(x => x.TeamExperience)
                .MaximumLength(2000).WithMessage("Team experience must not exceed 2000 characters.")
                .Matches(TextPattern).WithMessage("Team experience contains invalid characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.TeamExperience));

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Revenue must be greater than or equal to 0.")
                .When(x => x.Revenue.HasValue);

            RuleFor(x => x.MarketSize)
                .GreaterThanOrEqualTo(0).WithMessage("Market size must be greater than or equal to 0.")
                .When(x => x.MarketSize.HasValue);

            // MVP + Growth required fields
            When(IsMvpOrGrowth, () =>
            {
                RuleFor(x => x.UniqueValueProposition)
                    .NotEmpty().WithMessage("Unique value proposition is required for MVP and Growth stages.");

                RuleFor(x => x.BusinessModel)
                    .NotEmpty().WithMessage("Business model is required for MVP and Growth stages.");

                RuleFor(x => x.KeySkills)
                    .NotEmpty().WithMessage("Key skills are required for MVP and Growth stages.");

                RuleFor(x => x.Competitors)
                    .NotEmpty().WithMessage("Competitors are required for MVP and Growth stages.");

            });

            // Growth specific required fields
            When(x => x.DevelopmentStage == DevelopmentStage.Growth, () =>
            {
                RuleFor(x => x.Revenue)
                    .NotNull().WithMessage("Revenue is required for Growth stage.")
                    .GreaterThan(0).WithMessage("Revenue must be greater than 0 for Growth stage.");

                RuleFor(x => x.MarketSize)
                    .NotNull().WithMessage("Market size is required for Growth stage.")
                    .GreaterThan(0).WithMessage("Market size must be greater than 0 for Growth stage.");

                RuleFor(x => x.TeamExperience)
                    .NotEmpty().WithMessage("Team experience is required for Growth stage.");
            });
        }

        private static bool IsMvpOrGrowth(CreateProjectRequest request)
        {
            return request.DevelopmentStage is DevelopmentStage.MVP or DevelopmentStage.Growth;
        }
    }
}
