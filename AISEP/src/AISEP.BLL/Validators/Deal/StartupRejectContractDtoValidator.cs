using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Deal
{
    public class StartupRejectContractDtoValidator : AbstractValidator<StartupRejectContractDto>
    {
        public StartupRejectContractDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do là bắt buộc khi startup từ chối hợp đồng.")
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.");
        }
    }
}
