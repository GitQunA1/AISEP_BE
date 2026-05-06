using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Connection
{
    public class CreateConnectionRequestDtoValidator : AbstractValidator<CreateConnectionRequestDto>
    {
        public CreateConnectionRequestDtoValidator()
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("Mã dự án phải là số dương.");

            RuleFor(x => x.Message)
                .MaximumLength(1000).WithMessage("Tin nhắn không được vượt quá 1000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Message));
        }
    }
}
