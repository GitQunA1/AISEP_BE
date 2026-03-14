using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Project
{
    public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
    {
        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x.ProjectName)
                .MaximumLength(255).WithMessage("Project name must not exceed 255 characters.");
            //.When(x => x.ProjectName is not null);

            RuleFor(x => x.DevelopmentStage)
                .IsInEnum().WithMessage("Development stage is not valid. Allowed: Idea, MVP, Growth.");
               // .When(x => x.DevelopmentStage.HasValue);

            RuleFor(x => x.ShortDescription)
                .MaximumLength(500).WithMessage("Short description must not exceed 500 characters.");
               // .When(x => x.ShortDescription is not null);

            RuleFor(x => x.ProblemStatement)
                .MaximumLength(2000).WithMessage("Problem statement must not exceed 2000 characters.");
               // .When(x => x.ProblemStatement is not null);

            RuleFor(x => x.SolutionDescription)
                .MaximumLength(2000).WithMessage("Solution description must not exceed 2000 characters.");
               // .When(x => x.SolutionDescription is not null);

            RuleFor(x => x.TargetCustomers)
                .MaximumLength(1000).WithMessage("Target customers must not exceed 1000 characters.");
               // .When(x => x.TargetCustomers is not null);

            RuleFor(x => x.UniqueValueProposition)
                .MaximumLength(1000).WithMessage("Unique value proposition must not exceed 1000 characters.");
               // .When(x => x.UniqueValueProposition is not null);

            RuleFor(x => x.MarketSize)
                .GreaterThanOrEqualTo(0).WithMessage("Market size must be a positive number.");
               //.When(x => x.MarketSize.HasValue);

            RuleFor(x => x.BusinessModel)
                .MaximumLength(1000).WithMessage("Business model must not exceed 1000 characters.");
               // .When(x => x.BusinessModel is not null);

            RuleFor(x => x.Revenue)
                .GreaterThanOrEqualTo(0).WithMessage("Revenue must be a positive number.");
               // .When(x => x.Revenue.HasValue);

            RuleFor(x => x.Competitors)
                .MaximumLength(1000).WithMessage("Competitors must not exceed 1000 characters.");
               // .When(x => x.Competitors is not null);

            RuleFor(x => x.TeamMembers)
                .MaximumLength(1000).WithMessage("Team members must not exceed 1000 characters.");
                //.When(x => x.TeamMembers is not null);

            RuleFor(x => x.KeySkills)
                .MaximumLength(1000).WithMessage("Key skills must not exceed 1000 characters.");
               // .When(x => x.KeySkills is not null);

            RuleFor(x => x.TeamExperience)
                .MaximumLength(1000).WithMessage("Team experience must not exceed 1000 characters."); 
               // .When(x => x.TeamExperience is not null);
        }
    }
}
