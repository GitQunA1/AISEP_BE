using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.SystemCommission
{
    public class UpdateSystemCommissionRequestValidator : AbstractValidator<UpdateSystemCommissionRequest>
    {
        private const string TextPattern = @"^[\p{L}\p{N}\s.,;:!?&()%'""-]*$";

        public UpdateSystemCommissionRequestValidator()
        {
            RuleFor(x => x.Percent)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Phần trăm hoa hồng phải nằm trong khoảng từ 0 đến 100.");

            RuleFor(x => x.Reason)
                .MaximumLength(1000).WithMessage("Lý do không được vượt quá 1000 ký tự.")
                .Matches(TextPattern).WithMessage("Lý do chứa ký tự không hợp lệ.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
