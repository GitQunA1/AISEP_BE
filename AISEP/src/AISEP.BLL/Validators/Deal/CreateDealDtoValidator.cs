using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class CreateDealDtoValidator : AbstractValidator<CreateDealDto>
    {
        public CreateDealDtoValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("ProjectId is required.")
                .GreaterThan(0).WithMessage("ProjectId must be a positive number.");
        }
    }
}
