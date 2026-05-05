using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Connection
{
    public class CreateConnectionRequestDtoValidator : AbstractValidator<CreateConnectionRequestDto>
    {
        public CreateConnectionRequestDtoValidator()
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("ProjectId phải là số dương.");

            RuleFor(x => x.Message)
                .MaximumLength(1000).WithMessage("Message không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Message));
        }
    }
}
