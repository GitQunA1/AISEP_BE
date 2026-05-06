using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class VerifyDealRequestDtoValidator : AbstractValidator<VerifyDealRequestDto>
    {
        public VerifyDealRequestDtoValidator()
        {
            RuleFor(x => x.IsConfirmed)
<<<<<<< HEAD
                .NotNull().WithMessage("Trạng thái xác nhận là bắt buộc.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.")
=======
                .NotNull().WithMessage("IsConfirmed là bắt buộc.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Reason không được vượt quá 2000 ký tự.")
>>>>>>> main
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
