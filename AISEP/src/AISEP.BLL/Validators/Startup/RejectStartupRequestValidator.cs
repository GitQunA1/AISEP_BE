using AISEP.BLL.DTOs.Requests;
using FluentValidation;

namespace AISEP.BLL.Validators.Startup
{
    public class RejectStartupRequestValidator : AbstractValidator<RejectStartupRequest>
    {
        public RejectStartupRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do là bắt buộc khi từ chối startup.")
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Lý do chứa ký tự không hợp lệ.")
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.");
        }
    }
}
