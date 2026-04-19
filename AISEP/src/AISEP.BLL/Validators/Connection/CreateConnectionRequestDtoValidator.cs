using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Connection
{
    public class CreateConnectionRequestDtoValidator : AbstractValidator<CreateConnectionRequestDto>
    {
        public CreateConnectionRequestDtoValidator()
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("ProjectId must be a positive number.");

            RuleFor(x => x.Message)
                .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Message));
        }
    }
}
