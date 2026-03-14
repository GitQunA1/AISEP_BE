using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.ProjectName)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(255).WithMessage("Project name must not exceed 255 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Project name contains invalid characters.");

            RuleFor(x => x.ShortDescription)
                .NotEmpty().WithMessage("Short description is required.")
                .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Short description contains invalid characters.");

            RuleFor(x => x.DevelopmentStage)
                .IsInEnum().WithMessage("Development stage is not valid. Allowed: Idea, MVP, Growth.");

            RuleFor(x => x.ProblemStatement)
                .NotEmpty().WithMessage("Problem statement is required.")
                .MaximumLength(2000).WithMessage("Problem statement must not exceed 2000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Problem statement contains invalid characters.");

            RuleFor(x => x.SolutionDescription)
                .NotEmpty().WithMessage("Solution description is required.")
                .MaximumLength(2000).WithMessage("Solution description must not exceed 2000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Solution description contains invalid characters.");

            RuleFor(x => x.TargetCustomers)
                .NotEmpty().WithMessage("Target customers is required.")
                .MaximumLength(1000).WithMessage("Target customers must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Target customers contains invalid characters.");

            RuleFor(x => x.UniqueValueProposition)
                .NotEmpty().WithMessage("Unique value proposition is required.")
                .MaximumLength(1000).WithMessage("Unique value proposition must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Unique value proposition contains invalid characters.");

            RuleFor(x => x.MarketSize)
                .NotNull().WithMessage("Market size is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Market size must be a positive number.");

            RuleFor(x => x.BusinessModel)
                .NotEmpty().WithMessage("Business model is required.")
                .MaximumLength(1000).WithMessage("Business model must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Business model contains invalid characters.");

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Revenue must be a positive number.")
                .When(x => x.Revenue.HasValue);

            RuleFor(x => x.Competitors)
                .NotEmpty().WithMessage("Competitor is required.")
                .MaximumLength(1000).WithMessage("Competitors must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Competitors contains invalid characters.");
                //.When(x => !string.IsNullOrWhiteSpace(x.Competitors));

            RuleFor(x => x.TeamMembers)
                .NotEmpty().WithMessage("Team members is required.")
                .MaximumLength(1000).WithMessage("Team members must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Team members contains invalid characters.");

            RuleFor(x => x.KeySkills)
                .NotEmpty().WithMessage("KeySkill is required.")
                .MaximumLength(1000).WithMessage("Key skills must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Key skills contains invalid characters.");
                //.When(x => !string.IsNullOrWhiteSpace(x.KeySkills));

            RuleFor(x => x.TeamExperience)
                .NotEmpty().WithMessage("TeamExperence is required.")
                .MaximumLength(1000).WithMessage("Team experience must not exceed 1000 characters.")
                .Matches("^[a-zA-Z0-9 .,!?'-]*$").WithMessage("Team experience contains invalid characters.");
                //.When(x => !string.IsNullOrWhiteSpace(x.TeamExperience));
        }
    }
}
