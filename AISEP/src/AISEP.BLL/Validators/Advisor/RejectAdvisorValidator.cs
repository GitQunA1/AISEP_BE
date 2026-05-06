using AISEP.BLL.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISEP.BLL.Validators.Advisor
{
    public class RejectAdvisorValidator : AbstractValidator<RejectAdvisorRequest>
    {
        public RejectAdvisorValidator()
        {
            RuleFor(x => x.Reason)
                .NotNull().WithMessage("Lý do không được để null.")
               //.Must(reason => !string.IsNullOrWhiteSpace(reason)).WithMessage("Lý do là bắt buộc.")
                .Matches("^[a-zA-Z0-9 .,!?'-àáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ]*$").WithMessage("Lý do chứa ký tự không hợp lệ.")
                .MaximumLength(2000).WithMessage("Lý do không được vượt quá 2000 ký tự.");
        }
    }
}
