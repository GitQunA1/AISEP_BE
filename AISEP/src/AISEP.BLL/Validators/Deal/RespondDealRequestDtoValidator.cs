using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class RespondDealRequestDtoValidator : AbstractValidator<RespondDealRequestDto>
    {
        public RespondDealRequestDtoValidator()
        {
            RuleFor(x => x.IsAccepted)
                .NotNull().WithMessage("Trạng thái chấp nhận là bắt buộc.");

            RuleFor(x => x.Reason)
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));

            When(x => x.IsAccepted.HasValue && !x.IsAccepted.Value, () =>
            {
                RuleFor(x => x.Reason)
                    .NotEmpty().WithMessage("Lý do là bắt buộc khi từ chối deal.");
            });
        }
    }
}
